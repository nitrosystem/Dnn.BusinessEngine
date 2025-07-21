function UpdateFieldDataSourceActionController(actionService, $scope) {
    this.execute = (action, params, defer) => {
        actionService.apiService.post("Module", "GetFieldDataSource", {
                ModuleId: action.ModuleId,
                ConnectionID: $scope.connectionID,
                FieldID: action.Settings.FieldID,
                PageIndex: actionService.expressionService.parseExpression(action.Settings.PageIndex, $scope),
                PageSize: actionService.expressionService.parseExpression(action.Settings.PageSize, $scope),
                Form: $scope.Form,
                PageUrl: document.URL,
            })
            .then((data) => {
                    $scope.Field[data.FieldName].DataSource = data.DataSource;

                    defer.resolve(data);
                },
                (error) => {
                    defer.reject(error.data);
                }
            );

        return defer.promise;
    }
}