ComponentRegistry.register('SwitchButton', function (controller) {
    this.init = (field) => {
        field.__ignoreReinit = true;
        this._hasAction = controller.hasFieldAction(field.Id, 'OnChange');
        field.onChange = this.onChange;
    }

    this.onChange = async (field, element) => {
        const currentValue = controller.get(field.FieldValueProperty);
        if (currentValue === false && field.Settings.ConvertFalseToNull && field.FieldValueProperty) {
            controller.set(field.FieldValueProperty, null);
        }

        if (this._hasAction) {
            element.disabled = true;
            await controller.callAction(field.Id, null, 'OnChange');
            element.disabled = false;
        }
    };
});