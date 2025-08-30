export function BindIf(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            const expr = attrs['b-if'];

            const comment = document.createComment('b-if');
            const parent = element.parentElement;

            const render = () => {
                const value = expressionService.evaluateExpression(expr, scope);
                if (typeof value == 'string') value = JSON.parse(value);

                if (value && !parent.contains(element))
                    parent.insertBefore(element, comment);
                else if (!value && parent.contains(element))
                    parent.replaceChild(comment, element);
            }

            render();

            const parts = expressionService.extractPropertyPaths(expr) ?? [];
            parts.forEach(item => {
                app.listenTo(item, scope, render);
            });
        }
    }
}