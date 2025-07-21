function DashboardMenuController(field, $scope, moduleController, $element) {
    this.init = () => {
    };

    $scope.onMenuClick = (pageId) => {
        moduleController.$rootScope.isLoadingDashboardModule = true;
        $scope.$emit('onGotoDashboardPage', { pageId: pageId, isUpdatePageParams: true });
    }
}