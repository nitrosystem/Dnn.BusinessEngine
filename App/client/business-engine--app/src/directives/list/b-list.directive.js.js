export function BindList(app, expressionService) {
    return {
        terminal: true,
        priority: 100,
        compile: function (attrs, element, scope, controller) {
            if (element.__b_list_processed) return;
            element.__b_list_processed = true;

            const expr = attrs['b-list'];

            // مثال: "item in items track by item.id"
            let [forPart] = expr.split(' track by ').map(s => s.trim());
            const [itemName, listName] = forPart.split(' in ').map(s => s.trim());

            const parent = element.parentElement;
            const placeholder = document.createComment(`b-list: ${listName}`);
            parent.insertBefore(placeholder, element);
            parent.removeChild(element);

            const render = () => {
                const list = expressionService.evaluateExpression(listName, scope) ?? [];

                let node = placeholder.nextSibling;
                while (node) {
                    const next = node.nextSibling;
                    if (node.nodeType === 1 && node.hasAttribute('b-list-clone')) {
                        parent.removeChild(node);
                    } else {
                        break;
                    }
                    node = next;
                }

                // بازسازی کامل لیست
                let lastNode = placeholder;
                list.forEach((item, index) => {
                    const clone = element.cloneNode(true);
                    clone.removeAttribute('b-list');
                    clone.setAttribute('b-list-clone', listName);
                    clone.setAttribute('b-list-index', index);

                    const childScope = Object.create(scope);
                    childScope[itemName] = item;
                    childScope.$index = index;

                    parent.insertBefore(clone, lastNode.nextSibling);
                    app.bindAttributes(clone, childScope);
                    app.detectElements(clone, childScope, controller, false, 'b-list');

                    lastNode = clone;
                });
            };

            render();
        }
    };
}
