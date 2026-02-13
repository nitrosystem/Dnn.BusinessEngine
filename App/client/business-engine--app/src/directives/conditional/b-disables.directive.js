export function BindDisabled(app, expressionService, globalService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_disabled_processed) return;
            element.__b_disabled_processed = true;

            const expr = attrs['b-disabled'];

            const render = () => {
                const value = expressionService.evaluateCondition(expr, scope);
                globalService.nextAnimationFrame(() => {
                    element.disabled = value
                });
            }

            render();

            const parts = expressionService.extractPropertyPaths(expr) ?? [];
            parts.forEach(item => {
                app.listenTo(item, scope, render);
            });
        }
    }
}