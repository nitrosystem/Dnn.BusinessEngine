export class ExpressionService {
    constructor() {
        this.ExpressionFunctions = {
            ToUpper: (value) => typeof value === 'string' ? value.toUpperCase() : value,
            ToLower: (value) => typeof value === 'string' ? value.toLowerCase() : value,
            Length: (value) => value?.length,
            // توابع دلخواه دیگه رو هم می‌تونی اضافه کنی
        };
    }

    evaluate(moduleData, expression) {
        if (this.isLiteral(expression))
            return this.parseLiteral(expression);

        const compiled = this.compileExpression(moduleData, expression);
        return compiled(moduleData);
    }

    compileExpression(moduleData, expression) {
        const funcMatch = expression.match(/^(\w+)\((.+)\)$/);
        if (funcMatch) {
            const funcName = funcMatch[1];
            const argPath = funcMatch[2];

            const func = this.ExpressionFunctions[funcName];
            if (!func) throw new Error(`Unknown '${funcName}'`);

            return (data) => {
                const value = this.resolvePath(data, argPath);
                return func(value);
            };
        }

        return (data) => this.resolvePath(data, expression);
    }

    resolvePath(moduleData, path) {
        if (!path) return null;

        const baseKeyMatch = path.match(/^(\w+)/);
        if (!baseKeyMatch) return null;

        const baseKey = baseKeyMatch[1];
        const root = moduleData[baseKey];
        if (!root) return null;

        const restPath = path.slice(baseKey.length);

        try {
            if (typeof root === 'object') {
                const wrapper = { [baseKey]: root };
                return this.evalJsonPath(wrapper, `${baseKey}${restPath}`);
            }
            return null;
        } catch (err) {
            throw new Error(`Invalid expression: ${path}\n${err.message}`);
        }
    }

    isLiteral(expression) {
        return /^["'].*["']$|^\d+(\.\d+)?$|^(true|false|null)$/i.test(expression);
    }

    parseLiteral(expression) {
        try {
            return JSON.parse(expression);
        } catch {
            return expression;
        }
    }

    evalJsonPath(obj, path) {
        // مثال ورودی: Courses[0].Trainer.FirstName
        const segments = path.replace(/\[(\w+)\]/g, '.$1').split('.');

        return segments.reduce((current, key) => {
            if (current && Object.prototype.hasOwnProperty.call(current, key)) {
                return current[key];
            }
            return undefined;
        }, obj);
    }
}