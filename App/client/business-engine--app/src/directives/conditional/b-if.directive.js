export function BindIf(app, expressionService) {
    return {
        terminal: true,
        priority: 100,
        compile: function (attrs, element, scope, controller) {

            if (element.__b_if_processed) return;
            element.__b_if_processed = true;

            let expr = attrs['b-if']?.trim();

            // literal detection
            if (/^(true|false|null|[0-9]+(\.[0-9]+)?)$/i.test(expr)) {
                expr = JSON.parse(expr.toLowerCase());
            }

            const comment = document.createComment("b-if");
            const parent = element.parentElement;
            if (!parent) return;

            const render = () => {
                const value = typeof expr === "string"
                    ? expressionService.evaluate(expr, scope)
                    : expr; // literal بدون parse

                if (value) {
                    if (!parent.contains(element))
                        parent.insertBefore(element, comment);
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