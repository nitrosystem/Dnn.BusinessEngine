export function BindModel(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_model_processed) return;
            element.__b_model_processed = true;

            const expr = attrs['b-model'];
            if (!expr) return;

            // Determining the control type.
            const tagName = element.tagName.toLowerCase();
            const type = (element.type || '').toLowerCase();

            const render = () => {
                const newValue = expressionService.evaluateExpression(expr, scope);

                if (tagName === 'input') {
                    if (type === 'checkbox') {
                        element.checked = !!newValue;
                    } else if (type === 'radio') {
                        element.checked = element.value == newValue;
                    } else {
                        // text, number, password, ...
                        if (element.value !== (newValue ?? '')) {
                            element.value = newValue ?? '';
                        }
                    }
                }
                else if (tagName === 'select') {
                    if (element.value !== (newValue ?? '')) {
                        element.value = newValue ?? '';
                    }
                }
                else {
                    // fallback for customize components
                    if (element.value !== (newValue ?? '')) {
                        element.value = newValue ?? '';
                    }
                }
            };

            // Change by user → update model
            const updateModel = (e) => {
                const { parent, key } = app.resolvePropReference(expr, scope);
                if (!parent) return;

                let newValue;

                if (tagName === 'input') {
                    if (type === 'checkbox') {
                        newValue = e.target.checked;
                    } else if (type === 'radio') {
                        if (e.target.checked) {
                            newValue = e.target.value;
                        } else {
                            return; // if uncheck, there is no need for set
                        }
                    } else {
                        newValue = e.target.value;
                    }
                }
                else if (tagName === 'select') {
                    newValue = e.target.value;
                }
                else {
                    newValue = e.target.value;
                }

                if (parent[key] !== newValue) {
                    parent[key] = newValue;
                }

                //app.notifyResolved(parent, key);
            };

            if (tagName === 'input' && (type === 'checkbox' || type === 'radio')) {
                element.addEventListener('change', updateModel);
            } else if (tagName === 'select') {
                element.addEventListener('change', updateModel);
            } else {
                element.addEventListener('input', updateModel);
            }

            render();

            app.listenTo(expr, scope, render);
        }
    }
}
