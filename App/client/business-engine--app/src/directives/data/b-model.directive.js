export function BindModel(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            const expr = attrs['b-model'];
            if (!expr) return;

            // نوع کنترل رو مشخص کنیم
            const tagName = element.tagName.toLowerCase();
            const type = (element.type || '').toLowerCase();

            // تابع به‌روزرسانی UI براساس model
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
                    // fallback برای کامپوننت‌های سفارشی
                    if (element.value !== (newValue ?? '')) {
                        element.value = newValue ?? '';
                    }
                }
            };

            // رویداد تغییر کاربر → update model
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
                            return; // اگر uncheck شد نیازی به set نیست
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
            };

            // وصل کردن event handler متناسب
            if (tagName === 'input' && (type === 'checkbox' || type === 'radio')) {
                element.addEventListener('change', updateModel);
            } else if (tagName === 'select') {
                element.addEventListener('change', updateModel);
            } else {
                element.addEventListener('input', updateModel);
            }

            // اولین بار render کن
            render();

            // گوش دادن به تغییرات مدل
            app.listenTo(expr, scope, render);
        }
    }
}
