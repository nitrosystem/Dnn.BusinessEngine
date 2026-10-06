ComponentRegistry.register('DatePicker', function (controller) {
    this.init = (field) => {
        field.__ignoreReinit = true;

        const hasAction = controller.hasFieldAction(field.Id, 'OnChange');
        const minDate = this._resolveDateValue(field.Settings.MinDate);
        const maxDate = this._resolveDateValue(field.Settings.MaxDate);
        const defaultDate = this._resolveDateValue(field.Settings.DefaultDate);

        const fp = flatpickr(`#datePicker_${field.Id}`, {
            dateFormat: field.Settings.Format || 'Y-m-d',
            minDate: minDate,
            maxDate: maxDate,
            defaultDate: defaultDate,
            disableMobile: field.Settings.DisableMobile || false,
            locale: {
                firstDayOfWeek: field.Settings.FirstDayOfWeek
            },
            allowInput: field.Settings.AllowInput || true,
            enableTime: field.Settings.EnableTime || false,
            onChange: async (selectedDates, dateStr) => {
                if (field.FieldValueProperty) {
                    const currentDate = selectedDates[0];
                    controller.set(field.FieldValueProperty, currentDate, true);
                }

                if (hasAction)
                    await controller.callAction(field.Id, null, 'OnChange');
            }
        });

        if (field.FieldValueProperty) {
            controller.listenTo(field.FieldValueProperty, (value) => {
                fp.setDate(value, false);
            });

            let currentValue = controller.get(field.FieldValueProperty);
            if (currentValue) {
                fp.setDate(currentValue, false);
            }
        }
    }

    /**
     * Resolves date value based on input type
     * @param {string|number} value - User input value
     * @param {object} controller - Controller for accessing fields
     * @returns {Date|string|null}
    */
    this._resolveDateValue = (value) => {
        // Empty value
        if (value === null || value === undefined || value === '') {
            return null;
        }
        else if (value === 'today') {
            return value;
        }

        const strValue = String(value).trim();
        // Example: "30", "-7", "+15"
        if (/^[+-]?\d+$/.test(strValue)) {
            const days = parseInt(strValue, 10);
            return new Date().fp_incr(days); // helper : Flatpickr
        }
        // Example: "2025-12-31", "2025/01/15", "2025-12-31T10:00:00"
        if (/^\d{4}[-/]\d{1,2}[-/]\d{1,2}/.test(strValue)) {
            const date = new Date(strValue);
            return isNaN(date.getTime()) ? null : date;
        }
        // Example: "Product.ExpiredDate", "MyDate"
        if (controller && typeof controller.get === 'function') {
            const resolved = controller.get(strValue);

            // If the resolved value itself was one of the above cases, return resolve.
            return resolved ? resolveDateValue(resolved, null) : null;
        }

        return null;
    }
});
