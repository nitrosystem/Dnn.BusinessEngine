export class DSLEngine {
    constructor(expressionService) {
        this.expressionService = expressionService;
        this.dslCommands = {}; // Map دستورات DSL به تابع اجرا
    }

    // ثبت دستور جدید DSL
    registerCommand(name, handler) {
        if (typeof handler !== 'function') throw new Error("Handler must be a function");
        this.dslCommands[name] = handler;
    }

    async run(script, context = {}) {
        const lines = script.split('\n');
        const stack = []; // مدیریت بلوک‌های شرطی
        let i = 0;

        while (i < lines.length) {
            const raw = lines[i];
            const line = raw.trim();
            i++;

            if (!line) continue;
            const indent = raw.match(/^\s*/)[0].length;

            // مدیریت بلوک‌های if/else با skip
            while (stack.length && stack[stack.length - 1].indent >= indent) {
                stack.pop();
            }
            const skip = stack.some(f => f.skip);

            // بلوک‌های شرطی
            let matched = false;

            // if expr
            let ifMatch = line.match(/^if\s+(.+)$/);
            if (ifMatch) {
                const expr = ifMatch[1].trim();
                const result = !skip && await this._eval(expr, context);
                stack.push({ indent, skip: !result, executed: result });
                matched = true;
            }

            // else if expr
            let elseIfMatch = line.match(/^else if\s+(.+)$/);
            if (elseIfMatch) {
                const expr = elseIfMatch[1].trim();
                const prev = stack.pop();
                const execute = !skip && !prev?.executed && await this._eval(expr, context);
                stack.push({ indent, skip: !execute, executed: execute });
                matched = true;
            }

            // else
            if (/^else$/i.test(line)) {
                const prev = stack.pop();
                const execute = !skip && !prev?.executed;
                stack.push({ indent, skip: !execute, executed: execute });
                matched = true;
            }

            if (matched) continue;

            // خطوط عادی
            if (!skip) await this._execDSL(line, context);
        }
    }

    async _eval(expr, context) {
        try {
            return await this.expressionService.evaluateExpression(expr, context);
        } catch (err) {
            console.error("Expression error:", expr, err);
            return undefined;
        }
    }

    async _execDSL(line, context) {
        const cmdMatch = line.match(/^([\w.$-]+)\s*:?(.+)?$/);
        if (!cmdMatch) {
            console.warn("Invalid DSL line:", line);
            return;
        }

        const cmd = cmdMatch[1].trim();
        const argsStr = (cmdMatch[2] || '').trim();
        const args = argsStr ? this._splitArgs(argsStr) : [];

        const handler = this.dslCommands[cmd];
        if (!handler) {
            console.warn("Unknown DSL command:", cmd);
            return;
        }

        await handler(args, context, this); // handler می‌تواند context و engine را بگیرد
    }

    // ساده برای جدا کردن آرگومان‌ها (برای مثال: prop = expr, prop2 = expr2)
    _splitArgs(argsStr) {
        const args = [];
        let current = '';
        let depth = 0;
        let inSingle = false;
        let inDouble = false;
        let escape = false;

        for (let i = 0; i < argsStr.length; i++) {
            const ch = argsStr[i];

            if (escape) {
                current += ch;
                escape = false;
                continue;
            }
            if (ch === '\\') { current += ch; escape = true; continue; }
            if (inSingle) { current += ch; if (ch === "'") inSingle = false; continue; }
            if (inDouble) { current += ch; if (ch === '"') inDouble = false; continue; }
            if (ch === "'") { current += ch; inSingle = true; continue; }
            if (ch === '"') { current += ch; inDouble = true; continue; }

            if (ch === '(') { depth++; current += ch; continue; }
            if (ch === ')') { depth = Math.max(0, depth - 1); current += ch; continue; }
            if (ch === ',' && depth === 0) { args.push(current.trim()); current = ''; continue; }

            current += ch;
        }
        if (current.trim()) args.push(current.trim());
        return args;
    }
}

//Advanced DSL Engine
// export class MiniDSLScriptEngineAdvanced {
//     constructor(expressionService) {
//         if (!expressionService || typeof expressionService.evaluateExpression !== 'function') {
//             throw new Error('MiniDSLScriptEngine requires expressionService.evaluateExpression(expr, context)');
//         }
//         this.expressionService = expressionService;

//         // سه نوع نگهدارندهٔ دستورات
//         this.dslCommands = {
//             global: new Map(),       // "redirect.to"
//             components: new Map(),   // "button.set"  => applies to all buttons
//             fields: new Map()        // "myGrid.refresh" => applies to specific instance name
//         };
//     }

//     // ---------- registration helpers ----------
//     registerCommand(name, handler) {
//         // legacy / global register
//         if (typeof handler !== 'function') throw new Error('handler must be function');
//         this.dslCommands.global.set(name, handler);
//     }

//     registerComponentCommand(componentType, commandName, handler) {
//         if (typeof handler !== 'function') throw new Error('handler must be function');
//         const key = `${componentType}.${commandName}`;
//         this.dslCommands.components.set(key, handler);
//     }

//     registerFieldCommand(fieldName, commandName, handler) {
//         if (typeof handler !== 'function') throw new Error('handler must be function');
//         const key = `${fieldName}.${commandName}`;
//         this.dslCommands.fields.set(key, handler);
//     }

//     // ---------- main runner ----------
//     async run(script, context = {}) {
//         const lines = (script || '').split('\n');
//         const stack = []; // for if/else blocks
//         let i = 0;

//         while (i < lines.length) {
//             const raw = lines[i];
//             const line = raw.trim();
//             i++;

//             if (!line) continue;
//             const indent = raw.match(/^\s*/)[0].length;

//             // unwind stack for blocks with indent >= current
//             while (stack.length && stack[stack.length - 1].indent >= indent) stack.pop();
//             const skip = stack.some(f => f.skip);

//             // conditional handling
//             let matched = false;

//             // if expr
//             let ifMatch = line.match(/^if\s+(.+)$/);
//             if (ifMatch) {
//                 const expr = ifMatch[1].trim();
//                 const result = !skip && !!(await this._eval(expr, context));
//                 stack.push({ indent, skip: !result, executed: result });
//                 matched = true;
//             }

//             // else if expr
//             let elseIfMatch = line.match(/^else if\s+(.+)$/);
//             if (elseIfMatch) {
//                 const expr = elseIfMatch[1].trim();
//                 const prev = stack.pop();
//                 const execute = !skip && !(prev && prev.executed) && !!(await this._eval(expr, context));
//                 stack.push({ indent, skip: !execute, executed: execute });
//                 matched = true;
//             }

//             // else
//             if (/^else$/i.test(line)) {
//                 const prev = stack.pop();
//                 const execute = !skip && !(prev && prev.executed);
//                 stack.push({ indent, skip: !execute, executed: execute });
//                 matched = true;
//             }

//             if (matched) continue;

//             // normal line
//             if (!skip) await this._execDSL(line, context);
//         }

//         return true;
//     }

//     // ---------- evaluate expression via expressionService ----------
//     async _eval(expr, context) {
//         try {
//             return await this.expressionService.evaluateExpression(expr, context);
//         } catch (err) {
//             console.error('Expression evaluation error:', expr, err);
//             // return undefined so condition treats missing value as falsy
//             return undefined;
//         }
//     }

//     // ---------- execute DSL line (dispatch to appropriate handler) ----------
//     async _execDSL(line, context) {
//         // cmd: maybe "button.set", "button.saveBtn.set", "redirect.to", etc
//         const cmdMatch = line.match(/^([\w.$-]+)\s*:?(.+)?$/);
//         if (!cmdMatch) {
//             console.warn('Invalid DSL line:', line);
//             return;
//         }

//         const fullCmd = cmdMatch[1].trim();
//         const argsStr = (cmdMatch[2] || '').trim();
//         const args = argsStr ? this._splitArgs(argsStr) : [];

//         const parts = fullCmd.split('.');
//         let handler = null;
//         let handlerContext = null; // additional info to pass (e.g., instance)

//         // 3-part: componentType.fieldName.command  (e.g. button.saveBtn.set)
//         if (parts.length === 3) {
//             const [componentType, fieldName, commandName] = parts;
//             const key = `${fieldName}.${commandName}`;
//             handler = this.dslCommands.fields.get(key);

//             // fallback: try component-level (componentType.commandName)
//             if (!handler) {
//                 const compKey = `${componentType}.${commandName}`;
//                 handler = this.dslCommands.components.get(compKey);
//             }

//             // set instance if available
//             if (context.fields && context.fields[fieldName]) {
//                 handlerContext = { instance: context.fields[fieldName] };
//             }
//         }

//         // 2-part: componentType.command  (e.g. button.set)
//         if (!handler && parts.length === 2) {
//             const [componentType, commandName] = parts;
//             const compKey = `${componentType}.${commandName}`;
//             handler = this.dslCommands.components.get(compKey);
//         }

//         // 1-part: global
//         if (!handler && parts.length === 1) {
//             handler = this.dslCommands.global.get(parts[0]);
//         }

//         if (!handler) {
//             console.warn('Unknown DSL command:', fullCmd);
//             return;
//         }

//         // call the handler with (argsArray, context, engine, handlerContext)
//         try {
//             await handler(args, context, this, handlerContext);
//         } catch (err) {
//             console.error('DSL handler error for', fullCmd, err);
//         }
//     }

//     // ---------- split arguments respecting quotes, parentheses, nested, etc ----------
//     _splitArgs(argsStr) {
//         const args = [];
//         let current = '';
//         let depth = 0;
//         let inSingle = false;
//         let inDouble = false;
//         let escape = false;

//         for (let i = 0; i < argsStr.length; i++) {
//             const ch = argsStr[i];

//             if (escape) {
//                 current += ch;
//                 escape = false;
//                 continue;
//             }
//             if (ch === '\\') { current += ch; escape = true; continue; }
//             if (inSingle) { current += ch; if (ch === "'") inSingle = false; continue; }
//             if (inDouble) { current += ch; if (ch === '"') inDouble = false; continue; }
//             if (ch === "'") { current += ch; inSingle = true; continue; }
//             if (ch === '"') { current += ch; inDouble = true; continue; }

//             if (ch === '(') { depth++; current += ch; continue; }
//             if (ch === ')') { depth = Math.max(0, depth - 1); current += ch; continue; }

//             // top-level comma splits args
//             if (ch === ',' && depth === 0) {
//                 args.push(current.trim());
//                 current = '';
//                 continue;
//             }

//             current += ch;
//         }
//         if (current.trim()) args.push(current.trim());
//         return args;
//     }

//     // ---------- key=value parser (only split on first '=') ----------
//     // returns { key, expr } or null if missing '='
//     parseKeyValue(pairStr) {
//         const idx = pairStr.indexOf('=');
//         if (idx === -1) return null;
//         const key = pairStr.substring(0, idx).trim();
//         const expr = pairStr.substring(idx + 1).trim();
//         return { key, expr };
//     }
// }
