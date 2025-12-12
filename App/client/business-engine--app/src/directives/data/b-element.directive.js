export function BindElement(app, expressionService) {
    return {
        compile: function (attrs, element, scope,controller) {
            if (element.__b_element_processed) return;
            element.__b_element_processed = true;

            const expr = attrs['b-element'];
            if (expr === '') return;

            const render = () => {
                element.setHTMLUnsafe(expr);
                app.detectElements(element, scope, controller, false);
            }

            render();

            if (attrs.listen !== 'false') app.listenTo(expr, scope, render);
        }
    }
}