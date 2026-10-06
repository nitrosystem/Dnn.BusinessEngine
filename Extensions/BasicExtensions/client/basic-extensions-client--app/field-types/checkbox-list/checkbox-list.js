ComponentRegistry.register('CheckboxList', function (controller, globalService) {
    this.init = (field) => {
        field.__ignoreReinit = true;

        if (field.FieldValueProperty) {
            let currentValue = controller.get(field.FieldValueProperty);
            if (currentValue) {
                if (!field.Settings.StorageType) {
                    currentValue = currentValue.toString().split(',')
                }
                else if (field.Settings.StorageType === 1 && typeof currentValue === 'string') {
                    currentValue = globalService.parseJson(currentValue);
                }

                for (const val of currentValue || []) {
                    field.DataSource.Items
                        .filter(i => i[field.DataSource.ValueField] === val)
                        .map(item => item.isSelected = true);
                }

                controller.notify(`field.${field.FieldName}.Datasource.Items`);
            }
        }

        field.onChange = this.onChange;
        this._hasAction = controller.hasFieldAction(field.Id, 'OnChange');
    }

    this.onChange = async (field, element) => {
        if (field.FieldValueProperty) {
            var values = [];
            for (const item of field.DataSource.Items.filter(i => i.isSelected)) {
                values.push(item[field.DataSource.ValueField]);
            }

            if (!field.Settings.StorageType || field.Settings.StorageType === 'delimited')
                controller.set(field.FieldValueProperty, values.join(), true);
            else if (field.Settings.StorageType === 'json')
                controller.set(field.FieldValueProperty, values, true);
        }

        if (this._hasAction) {
            element.disabled = true;
            await controller.callAction(field.Id, null, 'OnChange');
            element.disabled = false;
        }
    };
});
