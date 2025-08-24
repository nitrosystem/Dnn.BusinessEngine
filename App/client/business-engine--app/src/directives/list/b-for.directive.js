export function BindFor(app, expressionService) {
    return {
        compile: function (attrs, element, controller) {
            const expr = attrs['b-for'];

            // Parse: "item in items track by item.id"
            let [forPart, trackByPart] = expr.split(' track by ').map(s => s.trim());
            const [itemName, listName] = forPart.split(' in ').map(s => s.trim());
            const trackByExpr = trackByPart || '$index';

            const parent = element.parentElement;
            const placeholder = document.createComment(`b-for: ${listName}`);
            parent.insertBefore(placeholder, element);
            parent.removeChild(element);

            const renderedMap = new Map(); // key → { clone, controller }

            const render = () => {
                const list = expressionService.evaluateExpression(listName, controller) ?? [];
                const newMap = new Map();

                let lastNode = placeholder;

                list.forEach((item, index) => {
                    // Evaluate trackBy key
                    let key;
                    if (trackByExpr === '$index') {
                        key = index;
                    } else {
                        const scopedEval = Object.create(controller);
                        scopedEval[itemName] = item;
                        scopedEval.$index = index;
                        key = expressionService.evaluateExpression(trackByExpr, scopedEval);
                    }

                    let record = renderedMap.get(key);

                    if (record) {
                        // Reuse existing clone
                        newMap.set(key, record);

                        // Update controller values
                        record.controller[itemName] = item;
                        record.controller.$index = index;

                        // Ensure correct order (insert after lastNode)
                        if (record.clone.previousSibling !== lastNode) {
                            parent.insertBefore(record.clone, lastNode.nextSibling);
                        }

                        lastNode = record.clone;
                    } else {
                        // Create new clone
                        const clone = element.cloneNode(true);
                        clone.removeAttribute('b-for');
                        clone.setAttribute('b-for-clone', listName);
                        clone.setAttribute('b-for-index', index);

                        // 🔑 Scoped controller: inherits from parent controller
                        const scopedController = Object.create(controller);
                        scopedController[itemName] = item;
                        scopedController.$index = index;

                        parent.insertBefore(clone, lastNode.nextSibling);

                        app.detectElements(clone, scopedController);

                        record = { clone, controller: scopedController };
                        newMap.set(key, record);

                        lastNode = clone;
                    }
                });

                // Remove old nodes not in new list
                renderedMap.forEach((record, key) => {
                    if (!newMap.has(key)) {
                        parent.removeChild(record.clone);
                    }
                });

                // Swap maps
                renderedMap.clear();
                newMap.forEach((v, k) => renderedMap.set(k, v));
            };

            render();

            app.listenTo(controller, listName, render);
        }
    };
}
