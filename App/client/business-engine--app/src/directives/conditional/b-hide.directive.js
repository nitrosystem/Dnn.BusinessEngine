export function BindHide(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_hide_processed) return;
            element.__b_hide_processed = true;

            const expr = attrs['b-show'];

            const render = () => {
                const value = expressionService.evaluateCondition(expr, scope);
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