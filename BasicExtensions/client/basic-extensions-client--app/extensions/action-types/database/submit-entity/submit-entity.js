function SubmitEntityActionController(actionService, $scope) {
    this.execute = (action, params, defer) => {
        if (action.Settings && typeof action.Settings == "object")
            action.Settings = JSON.stringify(action.Settings);

        const postData = {
            ActionId: action.ActionId,
            ModuleId: action.ModuleId,
            ParentID: action.ParentID,
            ServiceId: action.ServiceId,
            FieldID: action.FieldID,
            ConnectionID: $scope.connectionID,
            PageUrl: document.URL,
            Settings: action.Settings,
            Params: params,
        };

        actionService.apiService.postApi("BusinessEngineNitroIAction", "Service", "CallSubmitEntityAction", postData).then((data) => {
                defer.resolve(data);
            },
            (error) => {
                defer.reject(error.data);
            }
        );

        return defer.promise;
    }
}