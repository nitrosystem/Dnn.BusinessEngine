export function BindHtml(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_text_processed) return;
            element.__b_text_processed = true;

            const expr = attrs['b-text'];
            if (expr === '') return;

            const render = () => {
                const value = expressionService.evaluateExpression(expr, scope)
                if (value !== null && value !== undefined)
                    element.setHTMLUnsafe(
                        typeof (value) == 'string'
                            ? value
                            : JSON.stringify(value));
                else {
                    if (attrs.literal == 'true')
                        element.setHTMLUnsafe(expr);
                }
            }

            render();

            if (attrs.listen !== 'false') app.listenTo(expr, scope, render);
        }
    }
}