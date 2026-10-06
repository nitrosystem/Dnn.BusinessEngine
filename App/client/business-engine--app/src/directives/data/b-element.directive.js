export function BindElement(app, expressionService) {
    return {
        compile: function (attrs, element, scope, controller) {
            if (element.__b_element_processed) return;
            element.__b_element_processed = true;

            const expr = attrs['b-element'];
            if (expr === '') return;

            const render = () => {
                const value = expressionService.evaluateExpression(expr, scope);
                element.setHTMLUnsafe(value);
                app.detectElements(element, scope, controller, false);
            }

            render();

            if (attrs.listen !== 'false') app.listenTo(expr, scope, render);
        }
    }
}