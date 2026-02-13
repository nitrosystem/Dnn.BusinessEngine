export class ExpressionService {
    constructor() {
        this.filters = {};
        this.functions = {};
    }

    evaluateCondition(expr, data) {
        if (!expr)
            return false;
        else if (typeof expr !== "string")
            return true;

        expr = expr.trim();
        const ast = this._parseExpression(expr);
        return this._evaluateExpressionTree(ast, data)
            ? true
            : false;
    }

    evaluateExpression(expr, data) {
        if (!expr || typeof expr !== "string") return expr;

        expr = expr.trim();

        const parts = expr.split('|').map(x => x.trim());
        const ast = this._parseExpression(parts[0]);
        let value = this._evaluateExpressionTree(ast, data);

        for (let i = 1; i < parts.length; i++) {
            const [filterName, ...args] = parts[i].split(':').map(s => s.trim());
            const filter = this.filters[filterName];
            if (!filter) continue;

            const parsedArgs = args.map(a => {
                if (a === "true") return true;
                else if (a === "false") return false;
                else if (!isNaN(a)) return Number(a);
                else if (a.startsWith("'") && a.endsWith("'")) return a.slice(1, -1);
                else return '';
            });

            value = filter(value, parsedArgs);
        }

        return value;
    }

    extractPropertyPaths(expression) {
        if (!expression) return [];

        const sanitized = this._stripStringLiterals(expression);

        // حذف functionName(
        const withoutFunctions = sanitized.replace(
            /\b[a-zA-Z_$][\w$]*\s*\(/g,
            "("
        );

        const regex = /\b[a-zA-Z_$][\w$]*(?:\.[a-zA-Z_$][\w$]*)*\b/g;
        const matches = withoutFunctions.match(regex) || [];

        const blacklist = new Set([
            'true', 'false', 'null', 'undefined',
            'NaN', 'Infinity'
        ]);

        return [...new Set(
            matches.filter(x => !blacklist.has(x))
        )];
    }

    _stripStringLiterals(expr) {
        return expr.replace(
            /(['"`])(?:\\.|(?!\1)[^\\])*\1/g,
            ''
        );
    }

    _parseExpression(expression) {
        expression = expression?.trim();
        if (!expression) return { type: "Literal", value: false };

        if (expression.startsWith("(") && expression.endsWith(")")) {
            if (this._isBalanced(expression))
                return this._parseExpression(expression.slice(1, -1));
        }

        // ------------- NEW: ternary ----------------
        const ternary = this._findTopLevelTernary(expression);
        if (ternary) {
            return {
                type: "Ternary",
                condition: this._parseExpression(ternary.condition),
                thenExpr: this._parseExpression(ternary.thenExpr),
                elseExpr: this._parseExpression(ternary.elseExpr)
            };
        }
        // -------------------------------------------

        let orIndex = this._findTopLevelOperator(expression, "||");
        if (orIndex !== -1) {
            return {
                type: "Or",
                left: this._parseExpression(expression.substring(0, orIndex)),
                right: this._parseExpression(expression.substring(orIndex + 2))
            };
        }

        let andIndex = this._findTopLevelOperator(expression, "&&");
        if (andIndex !== -1) {
            return {
                type: "And",
                left: this._parseExpression(expression.substring(0, andIndex)),
                right: this._parseExpression(expression.substring(andIndex + 2))
            };
        }

        // ⬅️ NOT اینجا
        if (expression.startsWith("!")) {
            let notCount = 0;
            while (expression.startsWith("!")) {
                notCount++;
                expression = expression.slice(1).trim();
            }

            let node = this._parseExpression(expression);
            return notCount % 2 === 1
                ? { type: "Not", expr: node }
                : node;
        }

        const compareMatch = expression.match(
            /^(.+?)(===|!==|==|!=|>=|<=|>|<)(.+)$/
        );
        if (compareMatch) {
            return {
                type: "Compare",
                left: this._parseValue(compareMatch[1].trim()),
                op: compareMatch[2],
                right: this._parseValue(compareMatch[3].trim())
            };
        }

        return this._parseValue(expression);
    }

    _findTopLevelOperator(expr, op) {
        let depth = 0;
        for (let i = 0; i < expr.length - (op.length - 1); i++) {
            const c = expr[i];
            if (c === "(") depth++;
            else if (c === ")") depth--;
            if (depth === 0 && expr.slice(i, i + op.length) === op) return i;
        }
        return -1;
    }

    _isBalanced(expr) {
        let depth = 0;
        for (let c of expr) {
            if (c === "(") depth++;
            else if (c === ")") depth--;
            if (depth < 0) return false;
        }
        return depth === 0;
    }

    _parseValue(raw) {
        if (!raw) return { type: "Literal", value: false };
        if (raw === "true") return { type: "Literal", value: true };
        if (raw === "false") return { type: "Literal", value: false };
        if (raw === "null") return { type: "Literal", value: null };
        if (!isNaN(raw)) return { type: "Literal", value: Number(raw) };
        if ((raw.startsWith("'") && raw.endsWith("'")) || (raw.startsWith('"') && raw.endsWith('"'))) return { type: "Literal", value: raw.slice(1, -1) };

        // 🟢 NEW: function call
        const fnMatch = raw.match(/^([a-zA-Z_$][\w$]*)\((.*)\)$/);
        if (fnMatch) {
            const [, name, argsRaw] = fnMatch;
            return {
                type: "FunctionCall",
                name,
                args: this._splitArguments(argsRaw).map(a => this._parseExpression(a))
            };
        }

        return { type: "Variable", name: raw };
    }

    _evaluateExpressionTree(node, data) {
        switch (node.type) {
            case "Literal": return node.value;
            case "Variable": return this._resolveVariable(node.name, data);
            case "Not": return !this._evaluateExpressionTree(node.expr, data);
            case "And": return this._evaluateExpressionTree(node.left, data) && this._evaluateExpressionTree(node.right, data);
            case "Or": return this._evaluateExpressionTree(node.left, data) || this._evaluateExpressionTree(node.right, data);
            case "Compare": {
                const left = this._evaluateExpressionTree(node.left, data);
                const right = this._evaluateExpressionTree(node.right, data);
                switch (node.op) {
                    case "===": return left === right;
                    case "!==": return left !== right;
                    case "==": return left == right;
                    case "!=": return left != right;
                    case ">": return left > right;
                    case "<": return left < right;
                    case ">=": return left >= right;
                    case "<=": return left <= right;
                }
            }
            case "FunctionCall": {
                const fn = this.functions?.[node.name];
                if (!fn) return undefined;

                const args = node.args.map(a =>
                    this._evaluateExpressionTree(a, data)
                );

                return fn(...args);
            }
            case "Ternary": {
                const cond = this._evaluateExpressionTree(node.condition, data);
                return cond
                    ? this._evaluateExpressionTree(node.thenExpr, data)
                    : this._evaluateExpressionTree(node.elseExpr, data);
            }
        }

        return false;
    }

    _resolveVariable(path, data) {
        if (!path) return undefined;
        let obj = data;

        // جدا کردن هر بخش با regex: a.b["x"].y[row.id]
        const parts = path.match(/([^[.\]]+)|\[(.*?)\]/g);
        for (let part of parts) {
            if (!obj) return undefined;

            if (part.startsWith("[")) {
                let inner = part.slice(1, -1).trim();
                if ((inner.startsWith('"') && inner.endsWith('"')) || (inner.startsWith("'") && inner.endsWith("'"))) {
                    inner = inner.slice(1, -1);
                } else {
                    inner = this.evaluateExpression(inner, data);
                }
                obj = obj[inner];
            } else {
                obj = obj[part];
            }
        }

        return obj;
    }

    _findTopLevelTernary(expr) {
        let depth = 0;
        let questionIndex = -1;

        for (let i = 0; i < expr.length; i++) {
            const c = expr[i];

            if (c === "(") depth++;
            else if (c === ")") depth--;

            if (depth !== 0) continue;

            if (c === "?" && questionIndex === -1) {
                questionIndex = i;
            }
        }

        if (questionIndex === -1) return null;

        // حالا باید : متناظر را پیدا کنیم
        depth = 0;
        for (let i = questionIndex + 1; i < expr.length; i++) {
            const c = expr[i];

            if (c === "(") depth++;
            else if (c === ")") depth--;

            if (depth === 0 && c === ":") {
                return {
                    condition: expr.substring(0, questionIndex).trim(),
                    thenExpr: expr.substring(questionIndex + 1, i).trim(),
                    elseExpr: expr.substring(i + 1).trim()
                };
            }
        }

        return null;
    }

    _splitArguments(expr) {
        const args = [];
        let depth = 0, current = "";

        for (let c of expr) {
            if (c === "," && depth === 0) {
                args.push(current.trim());
                current = "";
                continue;
            }
            if (c === "(") depth++;
            if (c === ")") depth--;
            current += c;
        }
        if (current.trim()) args.push(current.trim());
        return args;
    }
}