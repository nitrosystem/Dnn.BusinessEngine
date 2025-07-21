function TextboxController(field, $scope, moduleController, $element) {
    this.init = () => {
        const $textbox = $element.find(`input[type="${field.Settings.InputType}"]`);
        _.forEach(field.Settings.Attributes, (attr) => {
            $textbox.attr(attr.Name, attr.Value);
        });
    };

    //#region textbox event methods

    $scope.bTextbox_onFocus = (field, $event) => {
        if (field.Actions && field.Actions.length)
            this.$scope.actionManagerService.callActionsByEvent(this.$scope, "OnTextboxFocus", field.Actions);

        if ($event) $event.stopPropagation();
    };

    $scope.bTextbox_onBlur = (field, $event) => {
        moduleController.validateField(field).then((isValid) => {
            if (field.Actions && field.Actions.length) moduleController.callActionByEvent(field.Actions, field.FieldID, 'OnTextboxBlur');
        });

        if ($event) $event.stopPropagation();
    };

    $scope.bTextbox_onKeypress = (field, $event) => {
        if ($event.which === 13) {
            if (field.Actions && field.Actions.length)
                moduleController.callActionByEvent(field.Actions, field.FieldID, "OnEnterKey");

            if (field.Settings.EnterAction)
                moduleController.callActionByActionId(field.Settings.EnterAction);

            if (field.Settings.EnterButtonClick) {
                var buttonField = moduleController.getFieldByID(field.Settings.EnterButtonClick);
                if (buttonField) moduleController.$scope.$broadcast('onButtonClick', { field: buttonField });
            }

            if ($event) {
                $event.preventDefault();
                return false;
            }
        }
    };

    $scope.bTextbox_onKeydown = (field, $event) => {
        if ($event.which === 13) {
            if (field.Actions && field.Actions.length)
                moduleController.callActionByEvent(field.Actions, field.FieldID, "OnEnterKey");

            if (field.Settings.EnterAction)
                moduleController.callActionByActionId(field.Settings.EnterAction);

            if (field.Settings.EnterButtonClick) {
                var buttonField = moduleController.getFieldByID(field.Settings.EnterButtonClick);
                if (buttonField) moduleController.$scope.$broadcast('onButtonClick', { field: buttonField });
            }

            if ($event) {
                $event.preventDefault();
                return false;
            }
        }
    };

    $scope.bTextbox_onKeyup = (field, $event) => {
        if ($event.which === 13) {
            if (field.Actions && field.Actions.length)
                moduleController.callActionByEvent(field.Actions, field.FieldID, "OnEnterKey");

            if (field.Settings.EnterAction)
                moduleController.callActionByActionId(field.Settings.EnterAction);

            if (field.Settings.EnterButtonClick) {
                var buttonField = moduleController.getFieldByID(field.Settings.EnterButtonClick);
                if (buttonField) moduleController.$scope.$broadcast('onButtonClick', { field: buttonField });
            }

            if ($event) {
                $event.preventDefault();
                return false;
            }
        }
    };

    //#endregion
}