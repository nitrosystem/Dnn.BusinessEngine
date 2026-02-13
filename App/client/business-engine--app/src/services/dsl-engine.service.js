export class DSLEngineService {
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