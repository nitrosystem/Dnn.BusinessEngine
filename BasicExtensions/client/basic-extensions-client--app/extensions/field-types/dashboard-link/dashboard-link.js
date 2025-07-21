function DashboardLinkController(field, $scope, moduleController, $element) {
    this.init = () => {
    };

    $scope.bDashboardLink_onClick = (pageName, params) => {
        const pageParams = params ? moduleController.expressionService.parseExpression(params, $scope) : "";
        $scope.$emit('onGotoDashboardPage', { pageName: pageName, params: pageParams, isUpdatePageParams: true });
    }
}
