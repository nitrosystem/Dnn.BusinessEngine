export function BindDate(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_date_processed) return;
            element.__b_date_processed = true;

            const expr = attrs['b-date'];
            const render = () => {
                const value = expressionService.evaluateExpression(expr, scope);

                try {
                    const format = attrs.format || 'YYYY-MM-DD HH:mm';
                    const m = moment(value); // strict parsing
                    if (m.isValid())
                        element.textContent = m.format(format);
                    else
                        element.textContent = '';

                } catch (error) {
                    element.textContent = value ?? '';

                    console.error(error);
                }
            }

            render();

            if (attrs.listen !== 'false') app.listenTo(expr, scope, render);
        }
    }
}