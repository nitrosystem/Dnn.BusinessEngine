function MessageBoxController(field, $scope, moduleController, $element) {
    this.init = () => {
        _.forEach(field.Settings.Messages, (message) => {
            _.forEach(message.ShowConditions, (condition) => {
                const matches = (condition.LeftExpression || '').match(/(\w+)(\.([^{},=\-\+]+))?$/gm);
                _.forEach(matches, function (m) {
                    const match = /(\w+)(\.([^{},=\-\+]+))?$/gm.exec(m);
                    const property = match[0];
                    $scope.$watch(property, function (newVal, oldVal) {
                        if (newVal != oldVal) {
                            message.IsShow = moduleController.expressionService.checkConditions(message.ShowConditions, $scope);
                            var $obj = moduleController.$compile('<span>' + message.Content +'</span>')($scope);
                            message.Content2 = $obj.html();
                        }
                    }, true);
                });
            });
        });
    }
}