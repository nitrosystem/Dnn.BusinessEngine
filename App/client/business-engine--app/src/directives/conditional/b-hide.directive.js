export function BindHide(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            const expr = attrs['b-show'];

            const render = () => {
                const value = expressionService.evaluateExpression(expr, scope);
                if (typeof value == 'string') value = JSON.parse(value);

                element.style.display = value ? '' : 'none';
            }

            render();

            const parts = expressionService.extractPropertyPaths(expr) ?? [];
            parts.forEach(item => {
                app.listenTo(item, scope, render);
            });
        }
    }
}