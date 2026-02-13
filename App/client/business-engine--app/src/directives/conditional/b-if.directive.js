export function BindIf(app, expressionService) {
    return {
        terminal: true,
        priority: 90,
        compile: function (attrs, element, scope, controller) {
            if (element.__b_if_processed) return;
            element.__b_if_processed = true;

            let expr = attrs['b-if']?.trim();

            const comment = document.createComment("b-if");
            const parent = element.parentElement;
            if (!parent) return;

            const render = () => {
                const value = expressionService.evaluateCondition(expr, scope);
                if (value) {
                    if (!parent.contains(element))
                        parent.insertBefore(element, comment);

                    app.bindAttributes(element, scope);
                    app.detectElements(element, scope, controller, false, 'b-if');
                } else if (parent.contains(element)) {
                    parent.replaceChild(comment, element);
                }
            };
            
            render();

            if (typeof expr === "string") {
                const parts = expressionService.extractPropertyPaths(expr) ?? [];
                parts.forEach(item => app.listenTo(item, scope, render));
            }
        }
    }
}