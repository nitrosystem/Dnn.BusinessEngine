export function BindClass(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_class_processed) return;
            element.__b_class_processed = true;

            const expr = attrs['b-class'];
            if (!expr) return;

            const parseItems = () => {
                let result = [];
                (expr.match(/'([^']+)'\s*:\s*([^,}]+)/gim) ?? []).forEach(m => {
                    const match = /'([^']+)'\s*:\s*([^,}]+)/i.exec(m);
                    if (match) {
                        result.push({ className: match[1], condition: match[2] });
                    }
                });
                return result;
            };

            const items = parseItems();

            const render = () => {
                items.forEach(item => {
                    const value = expressionService.evaluateCondition(item.condition, scope);
                    if (value && !element.classList.contains(item.className))
                        element.classList.add(item.className);
                    else if (!value && element.classList.contains(item.className))
                        element.classList.remove(item.className);
                });
            };

            render();

            // dependency tracking
            items.forEach(item => {
                const parts = expressionService.extractPropertyPaths(item.condition);
                parts.forEach(path => {
                    app.listenTo(path, scope, render);
                });
            });
        }
    }
}