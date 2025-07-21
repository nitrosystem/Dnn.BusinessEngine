function SwitchButtonController(field, $scope, moduleController) {
    this.init = () => {
    }

    $scope.bSwitchButton_onChange = (field) => {
        if (field.Value === false && field.Settings.SetNullForFalse) delete field.Value;
    };
}