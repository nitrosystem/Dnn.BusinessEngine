ComponentRegistry.register('Checkbox', function (controller) {
    this.init = (field) => {
        field.__ignoreReinit = true;
        field.onChange = this.onChange;
        this._hasAction = controller.hasFieldAction(field.Id, 'OnChange');
    }

    this.onChange = async (field, element) => {
        const currentValue = controller.get(field.FieldValueProperty);
        if (currentValue === false && field.Settings.ConvertFalseToNull && field.FieldValueProperty) {
            controller.set(field.FieldValueProperty, null, true);
        }

        if (this._hasAction) {
            element.disabled = true;
            await controller.callAction(field.Id, null, 'OnChange');
            element.disabled = false;
        }
    };
});
