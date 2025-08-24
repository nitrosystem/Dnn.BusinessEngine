export function BindClick(app, expressionService) {
    return {
        compile: function (attrs, element, scope, rootScope) {
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
                                args.push(_.get(scope, expression));
                        });
                    }

                    if (typeof scope[fnName] === "function") {
                        scope[fnName](...args);
                    }
                } catch (err) {
                    console.error("b-click error:", err);
                }
            });

            function get(obj, path, defaultValue = undefined) {
                return path.split('.').reduce((acc, key) => {
                    return acc && acc[key] !== undefined ? acc[key] : undefined;
                }, obj) ?? defaultValue;
            }
        }
    }
}