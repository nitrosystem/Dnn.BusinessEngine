ComponentRegistry.register('Textbox', function (controller) {
    this.init = (field) => {
        field.__ignoreReinit = true;

        const textboxElement = document.querySelector(`#textbox_${field.Id}`);

        if (field.Settings.TriggerButtonClickOnEnter) {
            textboxElement.addEventListener('keyup', (event) => {
                if (event.key === 'Enter') {
                    const buttonField = controller.getFieldById(field.Settings.TriggerButtonClickOnEnter);
                    const buttonElement = document.querySelector(`#button_${buttonField.Id}`);
                    buttonField.onClick(buttonField, buttonElement);
                }
            });
        }

        const hasAction = controller.hasFieldAction(field.Id, 'OnChange');
        if (hasAction) {
            textboxElement.addEventListener('change', async (event) => {
                await controller.callAction(field.Id, null, 'OnChange');
            });
        }
    }
});
