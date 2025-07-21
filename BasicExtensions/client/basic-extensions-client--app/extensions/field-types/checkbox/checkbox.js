function CheckboxController(field, $scope, moduleController) {
    this.init = () => {}

    $scope.bCheckbox_onChange = (field) => {
        if (field.Value === false && field.Settings.SetNullForFalse) delete field.Value;
    };
}