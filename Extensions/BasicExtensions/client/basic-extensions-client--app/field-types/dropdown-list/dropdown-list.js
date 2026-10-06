ComponentRegistry.register('DropdownList', function (controller) {
    this.init = (field) => {
        field.__ignoreReinit = true;
        field.onChange = this.onChange;
        this._hasAction = controller.hasFieldAction(field.Id, 'OnChange');
    }

    this.onChange = async (field, element) => {
        if (this._hasAction) {
            element.disabled = true;
            await controller.callAction(field.Id, null, 'OnChange');
            element.disabled = false;
        }
    }
});
