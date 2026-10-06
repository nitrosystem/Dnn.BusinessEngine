export function BindChange(app, expressionService) {
    return {
        compile: function (attrs, element, scope, controller) {
            if (element.__b_change_processed) return;
            element.__b_change_processed = true;

            const expr = attrs['b-change'];
            const fnName = expr.split('(')[0].trim();

            let argsExpr = expr.includes('(')
                ? expr.substring(expr.indexOf('(') + 1, expr.lastIndexOf(')'))
                : "";

            element.addEventListener('change', () => {
                let args = [];
                if (argsExpr) {
                    argsExpr.split(',').forEach((expression) => {
                        if (/^["'].*["']$|^\d+(\.\d+)?$|^(true|false|null)$/i.test(expression))
                            args.push(JSON.parse(expression));
                        else
                            args.push(expressionService.evaluateExpression(expression, scope));
                    });
                }

                args.push(element);

                const { parent, key } = app.resolvePropReference(fnName, scope);
                if (typeof parent[key] === "function") {
                    parent[key](...args);
                }
            });
        }
    }
}