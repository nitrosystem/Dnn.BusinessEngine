function ButtonController(field, $scope, moduleController, $element) {
    this.init = () => {
        _.forEach(field.Settings.LoadingConditions, (condition) => {
            const matches = (condition.LeftExpression || '').match(/(\w+)(\.([^{},=\-\+]+))?$/gm);
            _.forEach(matches, function (m) {
                const match = /(\w+)(\.([^{},=\-\+]+))?$/gm.exec(m);
                const property = match[0];
                $scope.$watch(property, function (newVal, oldVal) {
                    if (newVal != oldVal) {
                        checkLoadingConditions(field);
                    }
                }, true);
            });
        });
    }

    $scope.bButton_onClick = (field, $event) => {
        if (field.Settings.ButtonType == 'submit')
            moduleController.validateForm().then((isValid) => callAction(field));
        else {
            moduleController.validatePanes(_.clone(field.Settings.ValidationPanes) || []).then(() => {
                moduleController.validateGroups(_.clone(field.Settings.ValidationGroups) || []).then(() => callAction(field));
            });
        }

        if ($event) $event.stopPropagation();
    };

    $scope.$on('onButtonClick' , (e, args) => {
        $scope.bButton_onClick(args.field);
    })

    function callAction(field) {
        if (field.Actions && field.Actions.length) moduleController.callActionByEvent(field.Actions, field.FieldID, 'OnButtonClick');
    }

    function checkLoadingConditions(field) {
        var $button = $element.find('button');

        const isShowLoading = moduleController.expressionService.checkConditions(field.Settings.LoadingConditions, $scope);
        if (isShowLoading) {
            $button.addClass(field.Settings.LoadingCssClass);
            $button.attr('disabled', true);
        }
        else {
            $button.removeClass(field.Settings.LoadingCssClass);
            $button.removeAttr('disabled');
        }
    }
}