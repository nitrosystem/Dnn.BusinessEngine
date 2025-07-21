function CheckboxListController(field, $scope, moduleController) {
    this.init = () => {
        if (field.Value) parseItemsByValue(field, feidl.Value);
    }

    $scope.bCheckBoxList_onChange = function (field) {
        let values = [];
        _.forEach(field.DataSource.Items, (item) => {
            if (item.Selected) values.push(item[field.Settings.DataSource.ValueField]);
        });

        field.Value = values;
    };

    $scope.$on(`onFieldValueChange_${fieldID}`, (params, args) => {
        parseItemsByValue(field, field.Value);
    });

    function parseItemsByValue(field, value) {
        if (value && typeof value == 'string')
            value = value.join(',');
        else if (value && value instanceof Array == false)
            value = [value];
        else if (!value)
            return;


        _.setProperty(field.DataSource.Items, 'Selected', false)

        _.forEach(value, (item) => {
            let val = item;
            if (typeof item == 'object') val = item[field.Settings.DataSource.ValueField];
            _.filter(field.DataSource.Items, (i) => { return i[field.Settings.DataSource.ValueField] == val }).map((item) => item.Selected = true);
        })
    }

    //this method writed by CHATGPT !! :)
    _.setProperty = (arr, property, value) => {
        _.forEach(arr, obj => _.set(obj, property, value));
    };

}