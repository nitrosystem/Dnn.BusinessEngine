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
                    const newClasses = item.className ?? '';
                    const value = expressionService.evaluateCondition(item.condition, scope);

                    if (newClasses.trim()) {
                        const classArray = newClasses.split(' ').filter(Boolean);

                        if (value) {
                            element.classList.add(...classArray);
                        } else {
                            element.classList.remove(...classArray);
                        }
                    }
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