export function BindReadonly(app, expressionService, globalService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_readonly_processed) return;
            element.__b_readonly_processed = true;

            const expr = attrs['b-readonly'];

            const render = () => {
                const readonly = 'readonly';
                const value = expressionService.evaluateCondition(expr, scope);
                if (value)
                    element.setAttribute(readonly, readonly);
                else if (element.hasAttribute(readonly))
                    element.removeAttribute(readonly);
            }

            render();

            const parts = expressionService.extractPropertyPaths(expr) ?? [];
            parts.forEach(item => {
                app.listenTo(item, scope, render);
            });
        }
    }
}