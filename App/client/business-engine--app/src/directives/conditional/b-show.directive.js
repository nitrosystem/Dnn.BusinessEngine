export function BindShow(app, expressionService, globalService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_show_processed) return;
            element.__b_show_processed = true;

            const expr = attrs['b-show'];

            const render = () => {
                const oldValue = globalService.isElementVisible(element);
                const value = expressionService.evaluateCondition(expr, scope);

                element.style.display = value ? '' : 'none';
                element.setAttribute('b-show-val', !!value);

                if (!oldValue && value) app.broadcast('onElementDisplayChange', { element, value });
            }

            render();

            const parts = expressionService.extractPropertyPaths(expr) ?? [];
            parts.forEach(item => {
                app.listenTo(item, scope, render);
            });
        }
    }
}