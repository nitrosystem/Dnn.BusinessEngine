ComponentRegistry.register('Textarea', function (controller) {
    this.init = (field) => {
        field.__ignoreReinit = true;

        const hasAction = controller.hasFieldAction(field.Id, 'OnChange');
        if (hasAction) {
            const textareaElement = document.querySelector(`#textarea_${field.Id}`);
            textareaElement.addEventListener('change', async (event) => {
                await controller.callAction(field.Id, null, 'OnChange');
            });
        }
    }
});
