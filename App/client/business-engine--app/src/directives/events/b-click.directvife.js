export function BindClick(app, expressionService) {
    return {
        compile: function (attrs, element, scope, controller) {
            // Prevent double-binding if this directive runs more than once on the same element
            if (element.__b_click_processed) return;
            element.__b_click_processed = true;

            const expr = attrs['b-click'];

            // Extract the function name (everything before the first opening parenthesis)
            const fnName = expr.split('(')[0].trim();

            // Extract the raw arguments string between the first "(" and the last ")"
            let argsExpr = expr.includes('(')
                ? expr.substring(expr.indexOf('(') + 1, expr.lastIndexOf(')'))
                : "";

            element.addEventListener('click', () => {
                let args = [];

                if (argsExpr) {
                    // Split arguments by comma, but keep object literals ({...}) intact
                    argsExpr.match(/({[^}]*}|[^,]+)/g)
                        .map(s => s.trim())
                        .forEach((expression) => {
                            const literal = parseArgLiteral(expression);
                            if (literal !== undefined) {
                                // It's a literal value (string, number, boolean, null, or object)
                                args.push(literal);
                            } else {
                                // Not a literal — evaluate it as an expression against the current scope
                                args.push(expressionService.evaluateExpression(expression, scope));
                            }
                        });
                }

                // Always pass the triggering DOM element as the last argument
                args.push(element);

                // Resolve the target function from the scope chain (e.g. "fn.set" -> parent object + key)
                const { parent, key } = app.resolvePropReference(fnName, scope);

                if (typeof parent[key] === "function") {
                    parent[key](...args);
                }
            });
        }
    };
}

/**
 * Attempts to parse a single argument expression as a literal value.
 * Supports single-quoted strings, double-quoted strings, numbers, booleans, null, and simple object literals.
 * Returns `undefined` if the expression is NOT a literal (meaning it should be evaluated against scope instead).
 */
function parseArgLiteral(expression) {
    // Match a string wrapped in either single or double quotes
    const strMatch = expression.match(/^(['"])([\s\S]*)\1$/);
    if (strMatch) {
        const quote = strMatch[1];
        const escapeRegex = quote === "'" ? /\\'/g : /\\"/g;

        return strMatch[2]
            // Temporarily protect escaped backslashes so they don't interfere with quote-unescaping
            .replace(/\\\\/g, '\u0000')
            // Unescape the escaped quote character matching the string's own delimiter
            .replace(escapeRegex, quote)
            // Restore protected backslashes
            .replace(/\u0000/g, '\\');
    }

    // Match numbers, booleans, null, or a simple object literal (still parsed via JSON.parse, since
    // these forms don't suffer from the single-quote issue that plain string literals have)
    if (/^\d+(\.\d+)?$|^(true|false|null)$|^{[^}]*}$/i.test(expression)) {
        return JSON.parse(expression);
    }

    // Not a recognizable literal — caller should treat it as an expression to evaluate
    return undefined;
}