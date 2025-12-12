export function BindLink(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_link_processed) return;
            element.__b_link_processed = true;

            const expr = attrs['b-link'];
            const render = () => {
                const value = expressionService.evaluateExpression(expr, scope);
                if (value !== null && value !== undefined)
                    element.setAttribute("href", value);
                else
                    element.setAttribute("href", expr);
            };

            render();
        }
    }
}