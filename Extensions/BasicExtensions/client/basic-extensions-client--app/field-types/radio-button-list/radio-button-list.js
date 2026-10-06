ComponentRegistry.register('RadioButtonList', function (controller) {
    this.init = (field) => {
        field.__ignoreReinit = true;
        this._hasAction = controller.hasFieldAction(field.Id, 'OnChange');
        field.onChange = this.onChange;
    }

     this.onChange = async (field, element) => {
        if (this._hasAction) {
            element.disabled = true;
            await controller.callAction(field.Id, null, 'OnChange');
            element.disabled = false;
        }
    }
});