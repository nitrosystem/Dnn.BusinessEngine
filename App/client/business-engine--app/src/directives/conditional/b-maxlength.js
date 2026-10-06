export function BindMaxLength(app, expressionService, globalService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_maxlength_processed) return;
            element.__b_maxlength_processed = true;
            const tagName = element.tagName.toLowerCase();

            // Only work for input and textarea 
            if (tagName !== 'input' && tagName !== 'textarea') return;

            const expr = attrs['b-maxlength'];
            if (!expr?.trim()) return;
            
            let currentMax = 0;

            // --- Calculation of maxlength ---
            const updateMax = () => {
                const value = expressionService.evaluateExpression(expr, scope);
                const parsed = parseInt(value, 10);
                currentMax = isNaN(parsed) || parsed < 0 ? 0 : parsed;
                globalService.nextAnimationFrame(() => {
                    //Setting native attributes for the first layer of protection
                    element.setAttribute('maxlength', currentMax);

                    //If the current value is over the limit, truncate it.
                    if (element.value.length > currentMax) {
                        trimValue();
                    }
                });
            };

            const trimValue = () => {
                const current = element.value;
                if (current.length > currentMax) {
                    element.value = current.slice(0, currentMax);

                    //Dispatching the input event to trigger other directives.
                    // (such as b-model) Get notified of the change
                    element.dispatchEvent(new Event('input', { bubbles: true }));
                }
            };

            // --- Event handlers ---
            // Preventing additional character typing
            const onBeforeInput = (e) => {
                if (!e.data) return; // backspace or delete

                const selStart = element.selectionStart ?? 0;
                const selEnd = element.selectionEnd ?? 0;
                const selectedLength = selEnd - selStart;
                const currentLength = element.value.length;

                //Final length after applying input
                const resultLength = currentLength - selectedLength + e.data.length;
                if (resultLength > currentMax) {
                    e.preventDefault();

                    //If a part of the new text is allow, enter that part.
                    const allowedChars = currentMax - (currentLength - selectedLength);
                    if (allowedChars > 0) {
                        const partialData = e.data.slice(0, allowedChars);
                        document.execCommand('insertText', false, partialData);
                    }
                }
            };

            // paste protection
            const onPaste = (e) => {
                e.preventDefault();

                const pastedText = (e.clipboardData || window.clipboardData).getData('text') || '';
                const selStart = element.selectionStart ?? 0;
                const selEnd = element.selectionEnd ?? 0;
                const selectedLength = selEnd - selStart;
                const currentLength = element.value.length;
                const available = currentMax - (currentLength - selectedLength);
                if (available <= 0) return;

                const safePaste = pastedText.slice(0, available);
                document.execCommand('insertText', false, safePaste);
            };

            // drop protection
            const onDrop = (e) => {
                e.preventDefault();
                const droppedText = e.dataTransfer?.getData('text') || '';
                const currentLength = element.value.length;
                const available = currentMax - currentLength;
                if (available <= 0) return;

                const safeDrop = droppedText.slice(0, available);

                // Focus on the element and insert text
                element.focus();
                document.execCommand('insertText', false, safeDrop);
            };

            //Final protection layer: If for any reason the value increases.
            const onInput = () => {
                if (element.value.length > currentMax) {
                    trimValue();
                }
            };

            // --- Register Events ---
            element.addEventListener('beforeinput', onBeforeInput);
            element.addEventListener('paste', onPaste);
            element.addEventListener('drop', onDrop);
            element.addEventListener('input', onInput);

            // --- Initial Assignment ---
            updateMax();

            // --- Listen to changing expression ---
            const parts = expressionService.extractPropertyPaths(expr) ?? [];
            parts.forEach(item => {
                app.listenTo(item, scope, updateMax);
            });
        }
    };
}
