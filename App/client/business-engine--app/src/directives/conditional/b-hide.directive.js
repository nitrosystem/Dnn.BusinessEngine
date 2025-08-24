export function BindHide(app, expressionService) {
    return {
        compile: function (attrs, element, controller) {
            const expr = attrs['b-show'];

            const render = () => {
                const value = expressionService.evaluateExpression(expr, controller);
                if (typeof value == 'string') value = JSON.parse(value);

                element.style.display = value ? '' : 'none';
            }

            render();

            const parts = expressionService.extractPropertyPaths(expr) ?? [];
            parts.forEach(item => {
                app.listenTo(controller, item, render);
            });
        }
    }
}