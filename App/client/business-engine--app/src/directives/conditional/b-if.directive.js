export function BindIf(app, expressionService) {
    return {
        compile: function (attrs, element, controller) {
            const expr = attrs['b-show'];

            const comment = document.createComment('b-if');
            const parent = el.parentElement;

            const render = () => {
                const value = expressionService.evaluateExpression(expr, controller);
                if (typeof value == 'string') value = JSON.parse(value);

                if (value && !parent.contains(el)) parent.insertBefore(el, comment);
                else if (!value && parent.contains(el)) parent.replaceChild(comment, el);
            }

            render();

            const parts = expressionService.extractPropertyPaths(expr) ?? [];
            parts.forEach(item => {
                app.listenTo(controller, item, render);
            });
        }
    }
}