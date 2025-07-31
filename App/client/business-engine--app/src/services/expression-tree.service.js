export class ExpressionTreeService {
    constructor() {
        this.ExpressionFunctions = {
            ToUpper: (x) => typeof x === 'string' ? x.toUpperCase() : x,
            ToLower: (x) => typeof x === 'string' ? x.toLowerCase() : x,
            Add: (a, b) => a + b,
            Length: (x) => x?.length ?? 0
            // توابع دیگه مثل Subtract, If, Contains رو هم می‌تونی اضافه کنی
        };
    }

    parseExpression(expression) {
        expression = expression.trim();

        // اگر literal باشه
        if (/^["'].*["']$|^\d+(\.\d+)?$|^(true|false|null)$/i.test(expression)) {
            return { type: 'Literal', value: JSON.parse(expression) };
        }

        // اگر فانکشن باشه
        const funcMatch = expression.match(/^(\w+)\((.*)\)$/);
        if (funcMatch) {
            const name = funcMatch[1];
            const args = this.splitArgs(funcMatch[2]).map(arg => this.parseExpression(arg.trim()));
            return { type: 'Function', name, args };
        }

        // در غیر این صورت یک مسیر به داده‌ست
        return { type: 'Path', value: expression };
    }

    evaluateExpressionTree(node, data) {
        switch (node.type) {
            case 'Literal':
                return node.value;

            case 'Path':
                return this.resolvePath(data, node.value);

            case 'Function':
                const func = this.ExpressionFunctions[node.name];
                if (!func) throw new Error(`Unknown '${node.name}'`);
                const args = node.args.map(arg => this.evaluateExpressionTree(arg, data));
                return func(...args);

            default:
                throw new Error(`Unsupported node type: ${node.type}`);
        }
    }

    splitArgs(argStr) {
        const args = [];
        let depth = 0, current = '';

        for (let i = 0; i < argStr.length; i++) {
            const ch = argStr[i];

            if (ch === ',' && depth === 0) {
                args.push(current);
                current = '';
            } else {
                if (ch === '(') depth++;
                if (ch === ')') depth--;
                current += ch;
            }
        }

        if (current) args.push(current);
        return args;
    }

    resolvePath(obj, path) {
        const segments = path.replace(/\[(\w+)\]/g, '.$1').split('.');
        return segments.reduce((acc, key) => acc?.[key], obj);
    }
}

