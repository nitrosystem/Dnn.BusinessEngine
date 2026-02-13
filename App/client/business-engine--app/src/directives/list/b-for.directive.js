export function BindFor(app, expressionService) {
    return {
        terminal: true,
        priority: 100,
        compile: function (attrs, element, scope, controller) {
            if (element.__b_for_processed) return;
            element.__b_for_processed = true;

            const expr = attrs['b-for'];

            const [forPart] = expr.split(' track by ').map(s => s.trim());
            const [itemName, listName] = forPart.split(' in ').map(s => s.trim());
            const parent = element.parentElement;
            const placeholder = document.createComment(`b-for: ${listName}`);
            parent.insertBefore(placeholder, element);
            parent.removeChild(element);

            const render = () => {
                const list = expressionService.evaluateExpression(listName, scope) ?? [];

                let node = placeholder.nextSibling;
                while (node) {
                    const next = node.nextSibling;
                    if (node.nodeType === 1 && node.hasAttribute('b-for-clone')) {
                        parent.removeChild(node);
                    } else {
                        break;
                    }
                    node = next;
                }

                let lastNode = placeholder;
                let index = 0;
                for (const item of list) {
                    const clone = element.cloneNode(true);
                    clone.removeAttribute('b-for');
                    clone.setAttribute('b-for-clone', listName);
                    clone.setAttribute('b-for-index', index);

                    const childScope = Object.create(scope);
                    childScope[itemName] = item;
                    childScope.index = index;

                    parent.insertBefore(clone, lastNode.nextSibling);
                    app.bindAttributes(clone, childScope);
                    app.detectElements(clone, childScope, controller, false, 'b-for');

                    lastNode = clone;
                    index++;
                };
            };

            render();

            app.listenTo(listName, scope, render);
        }
    };
}
