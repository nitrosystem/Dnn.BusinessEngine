export function BindText(app, expressionService) {
    return {
        compile: function (attrs, element, controller) {
            const expr = attrs['b-text'];
            const render = () => {
                const value = expressionService.evaluateExpression(expr, controller)
                if (value !== null && value !== undefined)
                    $(element).html(
                        typeof (value) == 'string'
                            ? value
                            : JSON.stringify(value)
                    );
            }

            render();

            app.listenTo(controller, expr, render);
        }
    }
}