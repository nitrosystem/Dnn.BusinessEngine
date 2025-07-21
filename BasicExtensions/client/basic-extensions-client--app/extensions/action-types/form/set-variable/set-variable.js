function SetVariableActionController(actionService, $scope) {
    this.execute = (action, params, defer) => {
        actionService.$timeout(() => {
            defer.resolve();
        })

        return defer.promise;
    }
}