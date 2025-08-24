export function BindClass(app, expressionService) {
    return {
        compile: function (attrs, element, controller) {
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
                    let value = expressionService.evaluateExpression(item.condition, controller);

                    if (typeof value === 'string') {
                        try { value = JSON.parse(value); } catch {}
                    }

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
                    app.listenTo(controller, path, render);
                });
            });
        }
    }
}