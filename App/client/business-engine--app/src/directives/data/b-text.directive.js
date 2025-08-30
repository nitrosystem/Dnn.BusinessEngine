export function BindText(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            const expr = attrs['b-text'];
            const render = () => {
                const value = expressionService.evaluateExpression(expr, scope)
                if (value !== null && value !== undefined)
                    $(element).html(
                        typeof (value) == 'string'
                            ? value
                            : JSON.stringify(value)
                    );
            }

            render();

            app.listenTo(expr, scope, render);
        }
    }
}