export function BindClick(app, expressionService) {
    return {
        compile: function (attrs, element, scope, controller) {
            const expr = attrs['b-click'];
            const fnName = expr.split('(')[0].trim();

            let argsExpr = expr.includes('(')
                ? expr.substring(expr.indexOf('(') + 1, expr.lastIndexOf(')'))
                : "";

            element.addEventListener('click', () => {
                try {
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

                    if (typeof controller[fnName] === "function") {
                        controller[fnName](...args);
                    }
                } catch (err) {
                    console.error("b-click error:", err);
                }
            });
        }
    }
}