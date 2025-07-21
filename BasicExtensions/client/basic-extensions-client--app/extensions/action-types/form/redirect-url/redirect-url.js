function RedirectUrlActionController(actionService, $scope) {
    this.execute = (action, params, defer) => {
        window.location = action.Settings.Url;
        defer.resolve();
    }
}