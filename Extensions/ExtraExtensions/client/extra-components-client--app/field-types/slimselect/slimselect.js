const slimselectDropdownController = function (controller, globalService) {
    this.init = (field) => {
        field.__ignoreReinit = true;

        if (field.DataSource) {
            const items = [];
            for (const item of field.DataSource.Items ?? []) {
                items.push(
                    {
                        text: item[field.DataSource.TextField],
                        value: item[field.DataSource.ValueField],
                    }
                )
            }

            items.unshift({ text: field.Settings.Placeholder, value: '', placeholder: 'true' });

            const select = new SlimSelect({
                select: `#slimselect_${field.FieldName}`,
                settings: {
                    allowDeselect: true,
                    searchPlaceholder: field.Settings.Placeholder
                },
                data: items,
                events: {
                    afterChange: (newVal) => {
                        field._ignoreWatch = true;

                        let value;
                        if (field.Settings.AllowMultiple) {
                            value = [];
                            for (const val of newVal) {
                                value.push(val.value === '' ? null : val.value);
                            }

                            if (!field.Settings.StorageType || field.Settings.StorageType === 'delimited')
                                controller.set(field.FieldValueProperty, value.join());
                            else if (field.Settings.StorageType === 'json')
                                controller.set(field.FieldValueProperty, value);
                        }
                        else {
                            value = newVal[0].value === '' ? null : newVal[0].value;
                            controller.set(field.FieldValueProperty, value);
                        }

                        globalService.nextMicroTask(() => field._ignoreWatch = false);
                    }
                }
            });

            if (field.FieldValueProperty) {
                controller.watch(field.FieldValueProperty, (value) => {
                    if (!field._ignoreWatch) select.setSelected(value);
                });

                let currentValue = controller.get(field.FieldValueProperty);
                if (currentValue) {
                    if (field.Settings.AllowMultiple) {
                        if (!field.Settings.StorageType) {
                            currentValue = currentValue.toString().split(',')
                        }
                        else if (field.Settings.StorageType === 1) {
                            if (typeof currentValue === 'string')
                                currentValue = globalService.parseJson(currentValue);
                        }
                    }

                    select.setSelected(currentValue);
                }
            }
        }
    }
}

ComponentRegistry.register("SlimSelectDropdown", slimselectDropdownController);
