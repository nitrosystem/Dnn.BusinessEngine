export class ScriptEngine {
    constructor() {

    }

    evaluateExpr(expr) {
        try {
            return Function("ctx", `with(ctx){ return (${expr}); }`)(this.context);
        } catch (err) {
            console.error("Expression error:", expr, err);
            return false;
        }
    }

    executeAction(line) {
        try {
            Function("ctx", `with(ctx){ ${line} }`)(this.context);
        } catch (err) {
            console.error("Action error:", line, err);
        }
    }

    run(script, context = {}) {
        this.context = context;

        const lines = script.split('\n');
        for (let i = 0; i < lines.length; i++) {
            let line = lines[i].trim();
            if (!line) continue;

            // ---- حالت تک‌خطی: if(expr) action ----
            const inlineIf = line.match(/^if\s*\((.*)\)\s*(.+)$/);
            if (inlineIf) {
                const expr = inlineIf[1].trim();
                const action = inlineIf[2].trim();

                if (this.evaluateExpr(expr)) {
                    this.executeAction(action);
                }
                continue;
            }

            // ---- حالت چند خطی: if(expr) ----
            const multiIf = line.match(/^if\s*\((.*)\)\s*$/);
            if (multiIf) {
                const expr = multiIf[1].trim();
                let indentActions = [];

                // جمع کردن اکشن‌های زیرین که با tab یا space شروع می‌شوند
                let j = i + 1;
                while (j < lines.length) {
                    const raw = lines[j];
                    if (/^\s+/.test(raw)) {
                        indentActions.push(raw.trim());
                        j++;
                    } else {
                        break;
                    }
                }

                // بردن pointer
                i = j - 1;

                if (this.evaluateExpr(expr)) {
                    indentActions.forEach(a => this.executeAction(a));
                }

                continue;
            }

            // ---- خط معمولی (بدون if) ----
            this.executeAction(line);
        }
    }
}


// export class ScriptEngine {
//     constructor(expressionService) {
//         this.expressionService = expressionService;
//         this.whiteList = {};
//         this.throwOnError = false;
//     }

//     async run(script, context = {}) {
//         try {
//             const lines = script
//                 .split('\n')
//                 .map(l => l.trim())
//                 .filter(l => l && !l.startsWith('//'));

//             for (const line of lines) {
//                 await this._runLine(line, context);
//             }
//         } catch (err) {
//             if (this.throwOnError) throw err;
//             return { __scriptError: true, message: err.message };
//         }
//     }

//     async _runLine(line, context) {
//         // ────────────── If statement ──────────────
//         const ifMatch = line.match(/^if\s*\((.*)\)\s*(.*)$/);
//         if (ifMatch) {
//             const expr = ifMatch[1].trim();
//             const action = ifMatch[2].trim();

//             const cond = await this._safeEvaluate(expr, context);
//             if (cond) {
//                 if (!action)
//                     throw new Error(`Invalid if statement: ${line}`);

//                 await this._runCall(action, context);
//             }
//             return;
//         }

//         // ─────────── Single method call ───────────
//         await this._runCall(line, context);
//     }

//     async _runCall(script, context) {
//         const { objectName, methodName, rawArgs } = this._parseCall(script);

//         this._assertAllowed(objectName, methodName);

//         const targetObj = context[objectName];
//         if (!targetObj)
//             throw new Error(`ScriptEngine: object '${objectName}' not found.`);

//         const fn = targetObj[methodName];
//         if (typeof fn !== 'function')
//             throw new Error(`ScriptEngine: method '${methodName}' not found.`);

//         const args = [];
//         for (const rawArg of rawArgs) {
//             const trimmed = rawArg.trim();
//             const v = await this._safeEvaluate(trimmed, context);
//             args.push(v);
//         }

//         return await fn.apply(targetObj, args);
//     }

//     async _safeEvaluate(expr, context) {
//         try {
//             return await this.expressionService.evaluateExpression(expr, context);
//         } catch (e) {
//             throw new Error(
//                 `Expression evaluation failed for "${expr}": ${e.message || e}`
//             );
//         }
//     }

//     _assertAllowed(objectName, methodName) {
//         if (!this.whiteList || Object.keys(this.whiteList).length === 0) return;
//         const allowed = this.whiteList[objectName];
//         if (!allowed)
//             throw new Error(`Object '${objectName}' is not allowed.`);
//         if (!allowed.includes(methodName))
//             throw new Error(`Method '${methodName}' of '${objectName}' is not allowed.`);
//     }

//     _parseCall(script) {
//         const s = script.replace(/;$/, '').trim();

//         const match = s.match(/^([A-Za-z_$][\w$]*)\.([A-Za-z_$][\w$]*)\(([\s\S]*)\)$/);
//         if (!match)
//             throw new Error(`Invalid call: ${script}`);

//         const objectName = match[1];
//         const methodName = match[2];
//         const argsBody = match[3].trim();

//         const rawArgs = this._splitArgs(argsBody);
//         return { objectName, methodName, rawArgs };
//     }

//     _splitArgs(argsStr) {
//         if (argsStr === '') return [];
//         const args = [];
//         let current = '';
//         let depth = 0;

//         for (let i = 0; i < argsStr.length; i++) {
//             const ch = argsStr[i];

//             if (ch === '(') depth++;
//             else if (ch === ')') depth--;
//             else if (ch === ',' && depth === 0) {
//                 args.push(current.trim());
//                 current = '';
//                 continue;
//             }

//             current += ch;
//         }

//         if (current.trim()) args.push(current.trim());
//         return args;
//     }
// }
