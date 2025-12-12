//12/1/2025 new 3
export class ExpressionService {
    constructor() {
        this.filters = {};
    }

    evaluate(expr, data) {
        if (!expr || typeof expr !== "string") return false;
        expr = expr.trim();
        const ast = this.parseExpression(expr);
        return this.evaluateExpressionTree(ast, data);
    }

    evaluateExpression(expr, data) {
        if (!expr || typeof expr !== "string") return expr;
        expr = expr.trim();

        const parts = expr.split('|').map(x => x.trim());
        const ast = this.parseExpression(parts[0]);
        let value = this.evaluateExpressionTree(ast, data);

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
        const regex = /([a-zA-Z_$][\w$]*(?:\.[a-zA-Z_$][\w$]*)+)/g;
        const matches = expression.match(regex);
        return [...new Set(matches)];
    }

    // -----------------------------
    // Parsing
    // -----------------------------
    parseExpression(expression) {
        expression = expression?.trim();
        if (!expression) return { type: "Literal", value: false };

        if (expression.startsWith("!")) {
            let notCount = 0;
            while (expression.startsWith("!")) {
                notCount++;
                expression = expression.slice(1).trim();
            }
            let node = this.parseExpression(expression);
            if (notCount % 2 === 1) return { type: "Not", expr: node };
            return node;
        }

        if (expression.startsWith("(") && expression.endsWith(")")) {
            if (this.isBalanced(expression)) return this.parseExpression(expression.slice(1, -1));
        }

        let orIndex = this.findTopLevelOperator(expression, "||");
        if (orIndex !== -1) return { type: "Or", left: this.parseExpression(expression.substring(0, orIndex)), right: this.parseExpression(expression.substring(orIndex + 2)) };

        let andIndex = this.findTopLevelOperator(expression, "&&");
        if (andIndex !== -1) return { type: "And", left: this.parseExpression(expression.substring(0, andIndex)), right: this.parseExpression(expression.substring(andIndex + 2)) };

        const compareMatch = expression.match(/^(.+?)(===|!==|==|!=|>=|<=|>|<)(.+)$/);
        if (compareMatch) return { type: "Compare", left: this.parseValue(compareMatch[1].trim()), op: compareMatch[2], right: this.parseValue(compareMatch[3].trim()) };

        return this.parseValue(expression);
    }

    findTopLevelOperator(expr, op) {
        let depth = 0;
        for (let i = 0; i < expr.length - (op.length - 1); i++) {
            const c = expr[i];
            if (c === "(") depth++;
            else if (c === ")") depth--;
            if (depth === 0 && expr.slice(i, i + op.length) === op) return i;
        }
        return -1;
    }

    isBalanced(expr) {
        let depth = 0;
        for (let c of expr) {
            if (c === "(") depth++;
            else if (c === ")") depth--;
            if (depth < 0) return false;
        }
        return depth === 0;
    }

    parseValue(raw) {
        if (!raw) return { type: "Literal", value: false };
        if (raw === "true") return { type: "Literal", value: true };
        if (raw === "false") return { type: "Literal", value: false };
        if (raw === "null") return { type: "Literal", value: null };
        if (!isNaN(raw)) return { type: "Literal", value: Number(raw) };
        if ((raw.startsWith("'") && raw.endsWith("'")) || (raw.startsWith('"') && raw.endsWith('"'))) return { type: "Literal", value: raw.slice(1, -1) };
        return { type: "Variable", name: raw };
    }

    evaluateExpressionTree(node, data) {
        switch (node.type) {
            case "Literal": return node.value;
            case "Variable": return this.resolveVariable(node.name, data);
            case "Not": return !this.evaluateExpressionTree(node.expr, data);
            case "And": return this.evaluateExpressionTree(node.left, data) && this.evaluateExpressionTree(node.right, data);
            case "Or": return this.evaluateExpressionTree(node.left, data) || this.evaluateExpressionTree(node.right, data);
            case "Compare": {
                const left = this.evaluateExpressionTree(node.left, data);
                const right = this.evaluateExpressionTree(node.right, data);
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
        }
        return false;
    }

    resolveVariable(path, data) {
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

    set(app, expr, scope, value) {
        try {
            const { parent, key } = app.resolvePropReference(expr, scope);
            parent[key] = value;
        } catch (error) {
            console.error(error);
        }
    }
}


//11/19/2025 - new 2
// export class ExpressionService {

//     evaluate(expr, data) {
//         if (!expr || typeof expr !== "string") return false;
//         expr = expr.trim();

//         const ast = this.parseExpression(expr);
//         return this.evaluateExpressionTree(ast, data);
//     }

//     evaluateExpression(expr, data) {
//         if (!expr || typeof expr !== "string") return false;
//         expr = expr.trim();

//         const ast = this.parseExpression(expr);
//         return this.evaluateExpressionTree(ast, data);
//     }

//     extractPropertyPaths(expression) {
//         const regex = /([a-zA-Z_$][\w$]*(?:\.[a-zA-Z_$][\w$]*)+)/g;
//         const matches = expression.match(regex);
//         return [...new Set(matches)];
//     }

//     // -----------------------------
//     // Parsing
//     // -----------------------------
//     parseExpression(expression) {
//         expression = expression.trim();

//         if (!expression) return { type: "Literal", value: false };

//         // -------------------------------------
//         // 1) NOT operator: !, !!, !!!, ...
//         // -------------------------------------
//         if (expression.startsWith("!")) {
//             let notCount = 0;
//             while (expression.startsWith("!")) {
//                 notCount++;
//                 expression = expression.slice(1).trim();
//             }

//             let node = this.parseExpression(expression);

//             if (notCount % 2 === 1) {
//                 return {
//                     type: "Not",
//                     expr: node
//                 };
//             }
//             return node;
//         }

//         // -------------------------------------
//         // 2) Parentheses
//         // -------------------------------------
//         if (expression.startsWith("(") && expression.endsWith(")")) {
//             if (this.isBalanced(expression)) {
//                 return this.parseExpression(expression.slice(1, -1));
//             }
//         }

//         // -------------------------------------
//         // 3) OR (||)
//         // -------------------------------------
//         let orIndex = this.findTopLevelOperator(expression, "||");
//         if (orIndex !== -1) {
//             return {
//                 type: "Or",
//                 left: this.parseExpression(expression.substring(0, orIndex)),
//                 right: this.parseExpression(expression.substring(orIndex + 2))
//             };
//         }

//         // -------------------------------------
//         // 4) AND (&&)
//         // -------------------------------------
//         let andIndex = this.findTopLevelOperator(expression, "&&");
//         if (andIndex !== -1) {
//             return {
//                 type: "And",
//                 left: this.parseExpression(expression.substring(0, andIndex)),
//                 right: this.parseExpression(expression.substring(andIndex + 2))
//             };
//         }

//         // -------------------------------------
//         // 5) Comparisons
//         // -------------------------------------
//         const compareMatch = expression.match(/^(.+?)(===|!==|==|!=|>=|<=|>|<)(.+)$/);
//         if (compareMatch) {
//             return {
//                 type: "Compare",
//                 left: this.parseValue(compareMatch[1].trim()),
//                 op: compareMatch[2],
//                 right: this.parseValue(compareMatch[3].trim())
//             };
//         }

//         return this.parseValue(expression);
//     }

//     // Find AND / OR only at top-level parentheses
//     findTopLevelOperator(expr, op) {
//         let depth = 0;

//         for (let i = 0; i < expr.length - (op.length - 1); i++) {
//             const c = expr[i];

//             if (c === "(") depth++;
//             else if (c === ")") depth--;

//             if (depth === 0 && expr.slice(i, i + op.length) === op) {
//                 return i;
//             }
//         }
//         return -1;
//     }

//     // Check balanced parentheses
//     isBalanced(expr) {
//         let depth = 0;
//         for (let c of expr) {
//             if (c === "(") depth++;
//             else if (c === ")") depth--;
//             if (depth < 0) return false;
//         }
//         return depth === 0;
//     }

//     // Parse literal or variable
//     parseValue(raw) {
//         if (!raw) return { type: "Literal", value: false };

//         if (raw === "true") return { type: "Literal", value: true };
//         if (raw === "false") return { type: "Literal", value: false };
//         if (raw === "null") return { type: "Literal", value: null };

//         if (!isNaN(raw)) return { type: "Literal", value: Number(raw) };

//         // string literal: 'abc' or "abc"
//         if ((raw.startsWith("'") && raw.endsWith("'")) ||
//             (raw.startsWith('"') && raw.endsWith('"'))) {
//             return {
//                 type: "Literal",
//                 value: raw.substring(1, raw.length - 1)
//             };
//         }

//         return { type: "Variable", name: raw };
//     }

//     // -----------------------------
//     // Evaluation
//     // -----------------------------
//     evaluateExpressionTree(node, data) {
//         switch (node.type) {

//             case "Literal":
//                 return node.value;

//             case "Variable":
//                 return this.resolveVariable(node.name, data);

//             case "Not":
//                 return !this.evaluateExpressionTree(node.expr, data);

//             case "And":
//                 return (
//                     this.evaluateExpressionTree(node.left, data) &&
//                     this.evaluateExpressionTree(node.right, data)
//                 );

//             case "Or":
//                 return (
//                     this.evaluateExpressionTree(node.left, data) ||
//                     this.evaluateExpressionTree(node.right, data)
//                 );

//             case "Compare":
//                 const left = this.evaluateExpressionTree(node.left, data);
//                 const right = this.evaluateExpressionTree(node.right, data);

//                 switch (node.op) {
//                     case "===": return left === right;
//                     case "!==": return left !== right;
//                     case "==": return left == right;
//                     case "!=": return left != right;
//                     case ">": return left > right;
//                     case "<": return left < right;
//                     case ">=": return left >= right;
//                     case "<=": return left <= right;
//                 }
//         }

//         return false;
//     }

//     // Resolve nested variables like a.b.c
//     resolveVariable(path, data) {
//         const parts = path.split(".");
//         let obj = data;
//         for (let p of parts) {
//             if (obj == null || typeof obj !== "object") return undefined;
//             obj = obj[p];
//         }
//         return obj;
//     }
// }

//11/19/2025 - new 1
// export class ExpressionService {
//     constructor() {
//         this.ExpressionFunctions = {
//             ToUpper: x => typeof x === 'string' ? x.toUpperCase() : x,
//             ToLower: x => typeof x === 'string' ? x.toLowerCase() : x,
//             Add: (a, b) => a + b,
//             Length: x => x?.length ?? 0
//         };
//     }

//     // -------------------------------
//     //      Public API
//     // -------------------------------
//     evaluateExpression(expr, data) {
//         const tree = this.parseExpression(expr.trim());
//         return this.evaluateExpressionTree(tree, data);
//     }

//     extractPropertyPaths(expression) {
//         const regex = /([a-zA-Z_$][\w$]*(?:\.[a-zA-Z_$][\w$]*)+)/g;
//         const matches = expression.match(regex);
//         return [...new Set(matches)];
//     }

//     // -------------------------------
//     //     Expression Parser
//     // -------------------------------
//     parseExpression(expression) {
//         expression = expression.trim();

//         // Remove outer parentheses
//         if (expression.startsWith("(") && expression.endsWith(")")) {
//             return this.parseExpression(expression.slice(1, -1).trim());
//         }

//         // ---------------------------------
//         // Handle NOT operator: !expr , !!expr , !!!expr
//         // ---------------------------------
//         if (expression.startsWith("!")) {
//             let notCount = 0;
//             while (expression.startsWith("!")) {
//                 notCount++;
//                 expression = expression.slice(1).trim();
//             }

//             const inner = this.parseExpression(expression);

//             if (notCount % 2 === 1) {
//                 return { type: "Not", expr: inner };
//             }
//             return inner; // !!expr simplifies to expr
//         }

//         const splitWithParens = (expr, op) => {
//             let depth = 0, parts = [], cur = "";
//             for (let i = 0; i < expr.length; i++) {
//                 const ch = expr[i];
//                 if (ch === '(') depth++;
//                 if (ch === ')') depth--;
//                 if (depth === 0 && expr.slice(i, i + op.length) === op) {
//                     parts.push(cur.trim());
//                     cur = "";
//                     i += op.length - 1;
//                 } else cur += ch;
//             }
//             if (cur) parts.push(cur.trim());
//             return parts.length > 1 ? parts : null;
//         };

//         // ---------------------------------
//         // Logical AND (&&)
//         // ---------------------------------
//         let andParts = splitWithParens(expression, "&&");
//         if (andParts) {
//             return {
//                 type: "Logical",
//                 op: "&&",
//                 parts: andParts.map(p => this.parseExpression(p))
//             };
//         }

//         // ---------------------------------
//         // Logical OR (||)
//         // ---------------------------------
//         let orParts = splitWithParens(expression, "||");
//         if (orParts) {
//             return {
//                 type: "Logical",
//                 op: "||",
//                 parts: orParts.map(p => this.parseExpression(p))
//             };
//         }

//         // ---------------------------------
//         // Comparisons (including === / !==)
//         // ---------------------------------
//         const compareMatch = expression.match(/^(.+?)(===|!==|==|!=|>=|<=|>|<)(.+)$/);
//         if (compareMatch) {
//             return {
//                 type: "Compare",
//                 op: compareMatch[2],
//                 left: this.parseExpression(compareMatch[1]),
//                 right: this.parseExpression(compareMatch[3])
//             };
//         }

//         // ---------------------------------
//         // Literals: string, number, true/false/null
//         // ---------------------------------
//         if (/^["'].*["']$/.test(expression)) {
//             const quote = expression[0];
//             const raw = expression.slice(1, -1);
//             const value = raw.replace(new RegExp(`\\\\${quote}`, "g"), quote);
//             return { type: "Literal", value };
//         }

//         if (/^\d+(\.\d+)?$/.test(expression)) {
//             return { type: "Literal", value: Number(expression) };
//         }

//         if (/^(true|false)$/i.test(expression)) {
//             return { type: "Literal", value: expression.toLowerCase() === "true" };
//         }

//         if (/^null$/i.test(expression)) {
//             return { type: "Literal", value: null };
//         }

//         // ---------------------------------
//         // Function Call
//         // ---------------------------------
//         const funcMatch = expression.match(/^(\w+)\((.*)\)$/);
//         if (funcMatch) {
//             const name = funcMatch[1];
//             const args = this.splitArgs(funcMatch[2]).map(a => this.parseExpression(a));
//             return { type: "Function", name, args };
//         }

//         // ---------------------------------
//         // Path (column.Name , user.info.id)
//         // ---------------------------------
//         return { type: "Path", value: expression };
//     }

//     // -------------------------------
//     //      Expression Evaluator
//     // -------------------------------
//     evaluateExpressionTree(node, data) {
//         switch (node.type) {
//             case "Literal":
//                 return node.value;

//             case "Path":
//                 return this.resolvePath(node.value, data);

//             case "Not":
//                 return !this.evaluateExpressionTree(node.expr, data);

//             case "Function":
//                 const fn = this.ExpressionFunctions[node.name];
//                 if (!fn) throw new Error(`Unknown function: ${node.name}`);
//                 const args = node.args.map(a => this.evaluateExpressionTree(a, data));
//                 return fn(...args);

//             case "Logical":
//                 if (node.op === "&&")
//                     return node.parts.every(p => this.evaluateExpressionTree(p, data));
//                 if (node.op === "||")
//                     return node.parts.some(p => this.evaluateExpressionTree(p, data));
//                 break;

//             case "Compare":
//                 const left = this.evaluateExpressionTree(node.left, data);
//                 const right = this.evaluateExpressionTree(node.right, data);

//                 switch (node.op) {
//                     case "===": return left === right;
//                     case "!==": return left !== right;
//                 }

//                 return this.compareValues(left, right, node.op);
//         }

//         throw new Error(`Unsupported node type: ${node.type}`);
//     }

//     // -------------------------------
//     //         Utility
//     // -------------------------------
//     splitArgs(str) {
//         let depth = 0, cur = "", args = [];
//         for (let ch of str) {
//             if (ch === ',' && depth === 0) {
//                 args.push(cur.trim());
//                 cur = "";
//                 continue;
//             }
//             if (ch === '(') depth++;
//             if (ch === ')') depth--;
//             cur += ch;
//         }
//         if (cur.trim()) args.push(cur.trim());
//         return args;
//     }

//     resolvePath(path, data) {
//         return path
//             .replace(/\[(\w+)\]/g, '.$1')
//             .split('.')
//             .reduce((acc, k) => acc?.[k], data);
//     }

//     // existing comparison logic (unchanged — safe)
//     compareValues(left, right, op) {
//         const isValidDate = v => !isNaN(Date.parse(v));
//         const isArray = Array.isArray;
//         const isEqual = (a, b) => JSON.stringify(a) === JSON.stringify(b);
//         const isEmpty = v => v === null || v === undefined ||
//             (typeof v === 'string' && v.trim() === '') ||
//             (isArray(v) && v.length === 0) ||
//             (typeof v === 'object' && !isArray(v) && Object.keys(v).length === 0);

//         switch (op) {
//             case '==': return left == right;
//             case '!=': return left != right;

//             case '>':
//                 if (isValidDate(left) && isValidDate(right)) return new Date(left) > new Date(right);
//                 return Number(left) > Number(right);

//             case '>=':
//                 if (isValidDate(left) && isValidDate(right)) return new Date(left) >= new Date(right);
//                 return Number(left) >= Number(right);

//             case '<':
//                 if (isValidDate(left) && isValidDate(right)) return new Date(left) < new Date(right);
//                 return Number(left) < Number(right);

//             case '<=':
//                 if (isValidDate(left) && isValidDate(right)) return new Date(left) <= new Date(right);
//                 return Number(left) <= Number(right);

//             case 'is-null': return isEmpty(left);
//             case 'not-null': return !isEmpty(left);
//         }

//         throw new Error(`Unknown operator: ${op}`);
//     }
// }

//11/19/2025 - old
// export class ExpressionService {
//     constructor() {
//         this.ExpressionFunctions = {
//             ToUpper: (x) => typeof x === 'string' ? x.toUpperCase() : x,
//             ToLower: (x) => typeof x === 'string' ? x.toLowerCase() : x,
//             Add: (a, b) => a + b,
//             Length: (x) => x?.length ?? 0
//             // توابع دیگه مثل Subtract, If, Contains رو هم می‌تونی اضافه کنی
//         };
//     }

//     compareValues(left, right, op) {
//         const isValidDate = (val) => !isNaN(Date.parse(val));
//         const isArray = Array.isArray;
//         const isEqual = (a, b) => JSON.stringify(a) === JSON.stringify(b);
//         const isEmpty = (val) => val === null || val === undefined ||
//             (typeof val === 'string' && val.trim() === '') ||
//             (isArray(val) && val.length === 0) ||
//             (typeof val === 'object' && !isArray(val) && Object.keys(val).length === 0);

//         switch (op.toLowerCase()) {
//             case '===':
//                 return isEqual(left, right);
//             case '==':
//                 return left == right;

//             case '!=':
//                 return !isEqual(left, right);

//             case 'in':
//                 return isArray(right) ? right.includes(left) : isEqual(left, right);

//             case 'not-in':
//                 return isArray(right) ? !right.includes(left) : !isEqual(left, right);

//             case 'like':
//                 return typeof right === 'string' && String(right).includes(String(left));

//             case 'not-like':
//                 return typeof right === 'string' && !String(right).includes(String(left));

//             case '>':
//                 if (isValidDate(left) && isValidDate(right))
//                     return new Date(left) > new Date(right);
//                 if (!isNaN(left) && !isNaN(right))
//                     return Number(left) > Number(right);
//                 return left > right;

//             case '>=':
//                 if (isValidDate(left) && isValidDate(right))
//                     return new Date(left) >= new Date(right);
//                 if (!isNaN(left) && !isNaN(right))
//                     return Number(left) >= Number(right);
//                 return left >= right;

//             case '<':
//                 if (isValidDate(left) && isValidDate(right))
//                     return new Date(left) < new Date(right);
//                 if (!isNaN(left) && !isNaN(right))
//                     return Number(left) < Number(right);
//                 return left < right;

//             case '<=':
//                 if (isValidDate(left) && isValidDate(right))
//                     return new Date(left) <= new Date(right);
//                 if (!isNaN(left) && !isNaN(right))
//                     return Number(left) <= Number(right);
//                 return left <= right;

//             case 'is-null':
//                 return isEmpty(left);

//             case 'not-null':
//                 return !isEmpty(left);

//             default:
//                 throw new Error(`Unknown operator: ${op}`);
//         }
//     }

//     extractIdentifiers(expression) {
//         const ast = acorn.parse(expression, { ecmaVersion: 2020 });
//         const identifiers = new Set();

//         function walk(node) {
//             switch (node.type) {
//                 case "Identifier":
//                     identifiers.add(node.name);
//                     break;
//                 case "MemberExpression":
//                     let parts = [];
//                     let current = node;
//                     while (current.type === "MemberExpression") {
//                         parts.unshift(current.property.name || current.property.value);
//                         current = current.object;
//                     }
//                     if (current.type === "Identifier") {
//                         parts.unshift(current.name);
//                         identifiers.add(parts.join("."));
//                     }
//                     break;
//             }

//             for (let key in node) {
//                 if (node[key] && typeof node[key] === "object") {
//                     walk(node[key]);
//                 }
//             }
//         }

//         walk(ast);

//         return Array.from(identifiers);
//     }

//     extractPropertyPaths(expression) {
//         const regex = /([a-zA-Z_$][\w$]*(?:\.[a-zA-Z_$][\w$]*)+)/g;
//         const matches = expression.match(regex);
//         return [...new Set(matches)];
//     }

//     evaluateExpression(expr, data) {
//         const expressionTree = this.parseExpression(expr, data);
//         return this.evaluateExpressionTree(expressionTree, data);
//     }

//     parseExpression(expression) {
//         expression = (expression ?? '').trim();

//         // پشتیبانی از ! (NOT)
//         if (expression.startsWith("!")) {
//             return {
//                 type: "Not",
//                 value: this.parseExpression(expression.slice(1).trim())
//             };
//         }

//         // NOT-handler
//         if (expression.startsWith("!")) {
//             const inner = expression.slice(1).trim();

//             return {
//                 type: "Not",
//                 expr: this.parseExpression(inner)
//             };
//         }

//         // اگر کل عبارت داخل پرانتز است → پرانتز بیرونی رو حذف کن
//         if (expression.startsWith("(") && expression.endsWith(")")) {
//             return this.parseExpression(expression.slice(1, -1).trim());
//         }

//         // کمکی: split با در نظر گرفتن پرانتز
//         const splitWithParens = (expr, op) => {
//             let depth = 0;
//             let parts = [];
//             let current = "";
//             for (let i = 0; i < expr.length; i++) {
//                 let ch = expr[i];
//                 if (ch === "(") depth++;
//                 if (ch === ")") depth--;
//                 if (depth === 0 && expr.slice(i, i + op.length) === op) {
//                     parts.push(current.trim());
//                     current = "";
//                     i += op.length - 1;
//                 } else {
//                     current += ch;
//                 }
//             }

//             if (current) parts.push(current.trim());
//             return parts.length > 1 ? parts : null;
//         };

//         // بررسی &&
//         let andParts = splitWithParens(expression, "&&");
//         if (andParts) {
//             return {
//                 type: "Logical",
//                 op: "&&",
//                 parts: andParts.map(e => this.parseExpression(e))
//             };
//         }

//         // بررسی ||
//         let orParts = splitWithParens(expression, "||");
//         if (orParts) {
//             return {
//                 type: "Logical",
//                 op: "||",
//                 parts: orParts.map(e => this.parseExpression(e))
//             };
//         }

//         // بررسی مقایسه
//         const compareMatch = expression.match(/^(.+?)(==|!=|>=|<=|>|<)(.+)$/);
//         if (compareMatch) {
//             return {
//                 type: "Compare",
//                 op: compareMatch[2],
//                 left: this.parseExpression(compareMatch[1].trim()),
//                 right: this.parseExpression(compareMatch[3].trim())
//             };
//         }

//         // literal
//         if (/^["'].*["']$/.test(expression)) {
//             // رشته‌هایی که در کوتیشن قرار دارن (تک یا دوتا)
//             // مثال‌ها:  'test'  یا  "test"
//             const quoteChar = expression[0];
//             const unquoted = expression.slice(1, -1);

//             // پشتیبانی از escape مثل 'it\'s ok'
//             const value = unquoted.replace(new RegExp(`\\\\${quoteChar}`, "g"), quoteChar);

//             return { type: "Literal", value };
//         }
//         else if (/^\d+(\.\d+)?$|^(true|false|null)$/i.test(expression)) {
//             // اعداد، true/false/null همچنان با JSON.parse تفسیر می‌شن
//             return { type: "Literal", value: JSON.parse(expression) };
//         }

//         // فانکشن
//         const funcMatch = expression.match(/^(\w+)\((.*)\)$/);
//         if (funcMatch) {
//             const name = funcMatch[1];
//             const args = this.splitArgs(funcMatch[2]).map(arg => this.parseExpression(arg.trim()));
//             return { type: "Function", name, args };
//         }

//         // مسیر (Path)
//         return { type: "Path", value: expression };
//     }

//     evaluateExpressionTree(node, data) {
//         switch (node.type) {
//             case 'Literal':
//                 return node.value;

//             case 'Path':
//                 return this.resolvePath(node.value, data);

//             case 'Function':
//                 const func = this.ExpressionFunctions[node.name];
//                 if (!func) throw new Error(`Unknown '${node.name}'`);
//                 const args = node.args.map(arg => this.evaluateExpressionTree(arg, data));
//                 return func(...args);

//             case 'Logical':
//                 if (node.op === '&&') {
//                     return node.parts.every(p => this.evaluateExpressionTree(p, data));
//                 }
//                 if (node.op === '||') {
//                     return node.parts.some(p => this.evaluateExpressionTree(p, data));
//                 }

//             case 'Not':
//                 return !this.evaluateExpressionTree(node.value, data);

//             case 'Compare':
//                 const leftVal = this.evaluateExpressionTree(node.left, data);
//                 const rightVal = this.evaluateExpressionTree(node.right, data);
//                 return this.compareValues(leftVal, rightVal, node.op);

//             default:
//                 throw new Error(`Unsupported node type: ${node.type}`);
//         }
//     }

//     splitArgs(argStr) {
//         const args = [];
//         let depth = 0, current = '';

//         for (let i = 0; i < argStr.length; i++) {
//             const ch = argStr[i];

//             if (ch === ',' && depth === 0) {
//                 args.push(current);
//                 current = '';
//             } else {
//                 if (ch === '(') depth++;
//                 if (ch === ')') depth--;
//                 current += ch;
//             }
//         }

//         if (current) args.push(current);
//         return args;
//     }

//     resolvePath(path, data) {
//         const segments = path.replace(/\[(\w+)\]/g, '.$1').split('.');
//         return segments.reduce((acc, key) => acc?.[key], data);
//     }
// }