import * as acorn from "acorn";

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
            case '==':
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

    extractIdentifiers(expression) {
        const ast = acorn.parse(expression, { ecmaVersion: 2020 });
        const identifiers = new Set();

        function walk(node) {
            switch (node.type) {
                case "Identifier":
                    identifiers.add(node.name);
                    break;
                case "MemberExpression":
                    let parts = [];
                    let current = node;
                    while (current.type === "MemberExpression") {
                        parts.unshift(current.property.name || current.property.value);
                        current = current.object;
                    }
                    if (current.type === "Identifier") {
                        parts.unshift(current.name);
                        identifiers.add(parts.join("."));
                    }
                    break;
            }

            for (let key in node) {
                if (node[key] && typeof node[key] === "object") {
                    walk(node[key]);
                }
            }
        }

        walk(ast);

        return Array.from(identifiers);
    }

    extractPropertyPaths(expression) {
        const regex = /([a-zA-Z_$][\w$]*(?:\.[a-zA-Z_$][\w$]*)+)/g;
        const matches = expression.match(regex);
        return [...new Set(matches)];
    }

    evaluateExpression(expr, scope) {
        const expressionTree = this.parseExpression(expr, scope);
        return this.evaluateExpressionTree(expressionTree, scope);
    }

    parseExpression(expression) {
        expression = (expression ?? '').trim();

        // اگر کل عبارت داخل پرانتز است → پرانتز بیرونی رو حذف کن
        if (expression.startsWith("(") && expression.endsWith(")")) {
            return this.parseExpression(expression.slice(1, -1).trim());
        }

        // کمکی: split با در نظر گرفتن پرانتز
        const splitWithParens = (expr, op) => {
            let depth = 0;
            let parts = [];
            let current = "";
            for (let i = 0; i < expr.length; i++) {
                let ch = expr[i];
                if (ch === "(") depth++;
                if (ch === ")") depth--;
                if (depth === 0 && expr.slice(i, i + op.length) === op) {
                    parts.push(current.trim());
                    current = "";
                    i += op.length - 1;
                } else {
                    current += ch;
                }
            }
            if (current) parts.push(current.trim());
            return parts.length > 1 ? parts : null;
        };

        // بررسی &&
        let andParts = splitWithParens(expression, "&&");
        if (andParts) {
            return {
                type: "Logical",
                op: "&&",
                parts: andParts.map(e => this.parseExpression(e))
            };
        }

        // بررسی ||
        let orParts = splitWithParens(expression, "||");
        if (orParts) {
            return {
                type: "Logical",
                op: "||",
                parts: orParts.map(e => this.parseExpression(e))
            };
        }

        // بررسی مقایسه
        const compareMatch = expression.match(/^(.+?)(==|!=|>=|<=|>|<)(.+)$/);
        if (compareMatch) {
            return {
                type: "Compare",
                op: compareMatch[2],
                left: this.parseExpression(compareMatch[1].trim()),
                right: this.parseExpression(compareMatch[3].trim())
            };
        }

        // literal
        if (/^["'].*["']$|^\d+(\.\d+)?$|^(true|false|null)$/i.test(expression)) {
            return { type: "Literal", value: JSON.parse(expression) };
        }

        // فانکشن
        const funcMatch = expression.match(/^(\w+)\((.*)\)$/);
        if (funcMatch) {
            const name = funcMatch[1];
            const args = this.splitArgs(funcMatch[2]).map(arg => this.parseExpression(arg.trim()));
            return { type: "Function", name, args };
        }

        // مسیر (Path)
        return { type: "Path", value: expression };
    }

    parseExpression1(expression) {
        expression = (expression ?? '').trim();

        if (expression.includes('&&')) {
            return {
                type: 'Logical',
                op: '&&',
                parts: expression.split('&&').map(e => this.parseExpression(e.trim()))
            };
        }

        if (expression.includes('||')) {
            return {
                type: 'Logical',
                op: '||',
                parts: expression.split('||').map(e => this.parseExpression(e.trim()))
            };
        }

        const compareMatch = expression.match(/^(.+?)(==|!=|>=|<=|>|<)(.+)$/);
        if (compareMatch) {
            return {
                type: 'Compare',
                op: compareMatch[2],
                left: this.parseExpression(compareMatch[1].trim()),
                right: this.parseExpression(compareMatch[3].trim())
            };
        }

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

            case 'Logical':
                if (node.op === '&&') {
                    return node.parts.every(p => this.evaluateExpressionTree(p, data));
                }
                if (node.op === '||') {
                    return node.parts.some(p => this.evaluateExpressionTree(p, data));
                }

            case 'Compare':
                const leftVal = this.evaluateExpressionTree(node.left, data);
                const rightVal = this.evaluateExpressionTree(node.right, data);
                return this.compareValues(leftVal, rightVal, node.op);

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