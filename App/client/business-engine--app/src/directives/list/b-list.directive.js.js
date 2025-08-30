export function BindList(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            const expr = attrs['b-list'];
            const [itemName, listName] = expr.split(' in ').map(s => s.trim());

            const parent = element.parentElement;
            const placeholder = document.createComment(`b-list: ${listName}`);
            parent.insertBefore(placeholder, element);
            parent.removeChild(element);

            parent.querySelectorAll(`[b-list-clone="${listName}"]`).forEach(e => e.remove());

            const list = expressionService.evaluateExpression(listName, scope) ?? [];
            list.forEach(item => {
                const clone = element.cloneNode(true);
                clone.removeAttribute('b-list');
                clone.setAttribute('b-list-clone', listName);

                let __scope = app.cloneClassInstance(scope);
                __scope[itemName] = item;

                parent.insertBefore(clone, placeholder);
                app.scanDirectives(clone, __scope);
            });
        }
    }
}
