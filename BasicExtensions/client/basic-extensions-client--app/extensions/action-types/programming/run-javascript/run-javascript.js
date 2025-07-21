function RunJavascriptActionController(actionService, $scope) {
    this.execute = (action, params, defer) => {
        return actionService.runScript($scope, action, params, action.Settings.Code, "");
    }
}