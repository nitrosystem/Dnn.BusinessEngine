export function BindFor(app, expressionService) {
    return {
        terminal: true,
        priority: 100,
        compile: function (attrs, element, scope, controller) {
            if (element.__b_for_processed) return;
            element.__b_for_processed = true;

            const expr = attrs['b-for'];

            // مثال: "item in items track by item.id"
            let [forPart] = expr.split(' track by ').map(s => s.trim());
            const [itemName, listName] = forPart.split(' in ').map(s => s.trim());

            const backup = element.cloneNode(true);

            const parent = element.parentElement;
            const placeholder = document.createComment(`b-for: ${listName}`);
            parent.insertBefore(placeholder, element);
            parent.removeChild(element);

            const render = () => {
                const list = expressionService.evaluateExpression(listName, scope) ?? [];

                // پاک کردن کل cloneهای قبلی
                let node = placeholder.nextSibling;
                while (node) {
                    const next = node.nextSibling;
                    if (node.nodeType === 1 && node.hasAttribute('b-for-clone')) {
                        parent.removeChild(node);
                    } else {
                        break; // اطمینان از اینکه از محدوده خارج نمی‌شیم
                    }
                    node = next;
                }

                // بازسازی کامل لیست
                let lastNode = placeholder;
                let index = 0;
                for (const item of list) {
                    const clone = element.cloneNode(true);
                    clone.removeAttribute('b-for');
                    clone.setAttribute('b-for-clone', listName);
                    clone.setAttribute('b-for-index', index);

                    const childScope = Object.create(scope);
                    childScope[itemName] = item;
                    childScope.$index = index;

                    parent.insertBefore(clone, lastNode.nextSibling);
                    app.detectElements(clone, childScope, controller, false, 'b-for');

                    app.on(`${listName}[${index}]`, (args) => {
                        const newElement = backup.cloneNode(true);
                        newElement.removeAttribute('b-for');
                        newElement.setAttribute('b-for-clone', listName);

                        index = args.index || index;
                        const newVal = args.newVal;
                        const child = Object.create(scope);
                        child[itemName] = newVal;
                        child.$index = index;
                        app.detectElements(newElement, child, controller, false, 'b-for');

                        element.replaceWith(newElement);
                    });

                    lastNode = clone;
                    index++;
                };
            };

            // اولین رندر
            render();

            // گوش دادن به تغییرات لیست
            app.listenTo(listName, scope, render);
        }
    };
}
