ComponentRegistry.register('Button', function (controller) {
    this.init = (field) => {
        field.__ignoreReinit = true;
        field.onClick = this.onClick;
        this._hasAction = controller.hasFieldAction(field.Id, 'OnClick');
    }

    this.onClick = async (field, element) => {
        let isValid = true;
        if (field.Settings.ButtonType === 'submit')
            isValid = await controller.validateForm();

        else
            isValid = field.Settings.ValidationPanes?.length
                ? await controller.validatePanes(field.Settings.ValidationPanes)
                : true;

        if (isValid) {
            await this._callAction(field, element);
        }
    }

    this._callAction = async (field, element) => {
        if (this._hasAction) {
            element.disabled = true;
            await controller.callAction(field.Id, null, 'OnClick');
            element.disabled = false;
        }
    }
});