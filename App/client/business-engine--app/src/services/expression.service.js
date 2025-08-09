export class ExpressionService {
    constructor() {
        this.ExpressionFunctions = {
            ToUpper: (x) => typeof x === 'string' ? x.toUpperCase() : x,
            ToLower: (x) => typeof x === 'string' ? x.toLowerCase() : x,
            Add: (a, b) => a + b,
            Length: (x) => x?.length ?? 0
            // توابع دیگه مثل Subtract, If, Contains رو هم می‌تونی اضافه کنی
        };
    }

    checkConditions(conditions, data) {
        if (!conditions || !conditions.length) return true;

        var andResult = true;

        var groups = _.groupBy(conditions, "GroupName");
        for (var key in groups) {
            var orResult = false;
            _.forEach(groups[key], (condition) => {
                const leftTree = this.parseExpression(condition.LeftExpression);
                const leftValue = this.evaluateExpressionTree(leftTree, data);

                const rightTree = this.parseExpression(condition.RightExpression);
                const rightValue = rightTree
                    ? this.evaluateExpressionTree(rightTree, data) :
                    undefined;

                const compareResult = this.compareValues(leftValue, rightValue, condition.EvalType);

                if (!orResult && compareResult) orResult = true;
            });
            if (!orResult) {
                andResult = false;
                break;
            }
        }

        return andResult;
    }

    compareValues(left, right, op) {
        const isValidDate = (val) => !isNaN(Date.parse(val));
        const isArray = Array.isArray;
        const isEqual = (a, b) => JSON.stringify(a) === JSON.stringify(b);
        const isEmpty = (val) => val === null || val === undefined ||
            (typeof val === 'string' && val.trim() === '') ||
            (isArray(val) && val.length === 0) ||
            (typeof val === 'object' && !isArray(val) && Object.keys(val).length === 0);

        switch (op.toLowerCase()) {
            case '=':
                return isEqual(left, right);
            case '!=':
                return !isEqual(left, right);

            case 'in':
                return isArray(right) ? right.includes(left) : isEqual(left, right);

            case 'not-in':
                return isArray(right) ? !right.includes(left) : !isEqual(left, right);

            case 'like':
                return typeof right === 'string' && String(right).includes(String(left));

            case 'not-like':
                return typeof right === 'string' && !String(right).includes(String(left));

            case '>':
                if (isValidDate(left) && isValidDate(right))
                    return new Date(left) > new Date(right);
                if (!isNaN(left) && !isNaN(right))
                    return Number(left) > Number(right);
                return left > right;

            case '>=':
                if (isValidDate(left) && isValidDate(right))
                    return new Date(left) >= new Date(right);
                if (!isNaN(left) && !isNaN(right))
                    return Number(left) >= Number(right);
                return left >= right;

            case '<':
                if (isValidDate(left) && isValidDate(right))
                    return new Date(left) < new Date(right);
                if (!isNaN(left) && !isNaN(right))
                    return Number(left) < Number(right);
                return left < right;

            case '<=':
                if (isValidDate(left) && isValidDate(right))
                    return new Date(left) <= new Date(right);
                if (!isNaN(left) && !isNaN(right))
                    return Number(left) <= Number(right);
                return left <= right;

            case 'is-null':
                return isEmpty(left);

            case 'not-null':
                return !isEmpty(left);

            default:
                throw new Error(`Unknown operator: ${op}`);
        }
    }

    parseExpression(expression) {
        if (expression === undefined || expression === null) return undefined;

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

