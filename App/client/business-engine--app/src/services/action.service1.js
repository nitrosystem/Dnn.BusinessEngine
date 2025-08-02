export class ActionService1 {
    constructor(
        $timeout,
        $q,
        $compile,
        $window,
        globalService,
        apiService,
        expressionService
    ) {
        this.$timeout = $timeout;
        this.$q = $q;
        this.$compile = $compile;
        this.$window = $window;
        this.globalService = globalService;
        this.apiService = apiService;
        this.expressionService = expressionService;
    }

    callActions(actions, moduleId, fieldId, eventName, data) {
        const defer = this.$q.defer();

        var buffer = this.createBuffer([], actions, moduleId, fieldId, eventName);

        this.callActionFromBuffer(buffer, defer, data).then((data) => {
            defer.resolve(data);
        }, (error) => {
            defer.resolve(error);
        });

        return defer.promise;
    }

    callActionByName(actionName, params, actions, data, ignoreChildActions) {
        const defer = this.$q.defer();

        const action = _.find(actions, (a) => {
            return a.ActionName == actionName;
        });

        action.Params = action.Params || [];

        if (typeof params == "object") {
            _.forOwn(params, (value, key) => {
                var param = _.find(action.Params, (p) => {
                    return p.ParamName == key;
                });
                if (param) param.ParamValue = value;
                else action.Params.push({ ParamName: key, ParamValue: value });
            });
        }

        const actionId = action.ActionId;

        var node = {
            Action: action,
            CompletedActions: [],
            SuccessActions: [],
            ErrorActions: [],
        };

        if (!ignoreChildActions) {
            var completedActions = this.getChildActions([], actions, actionId, 0);
            if (completedActions.length) node.CompletedActions = completedActions;
            var successActions = this.getChildActions([], actions, actionId, 1);
            if (successActions.length) node.SuccessActions = successActions;
            var errorActions = this.getChildActions([], actions, actionId, 2);
            if (errorActions.length) node.ErrorActions = errorActions;
        }

        this.callActionFromBuffer([node], defer, data).then((data) => {
            defer.resolve(data);
        }, (error) => {
            defer.reject(error);
        });

        return defer.promise;
    }

    createBuffer(buffer, moduleActions, moduleId, fieldId, eventName) {
        const actions = _.filter(moduleActions, (action) => {
            return (
                action.ModuleId == moduleId &&
                (!fieldId || action.FieldId == fieldId) &&
                action.Event == eventName
            );
        });

        _.forEach(actions, (action) => {
            var node = {
                Action: action,
                CompletedActions: [],
                SuccessActions: [],
                ErrorActions: [],
            };

            this.getChildActions(node.CompletedActions, moduleActions, action.ActionId, 0);
            this.getChildActions(node.SuccessActions, moduleActions, action.ActionId, 1);
            this.getChildActions(node.ErrorActions, moduleActions, action.ActionId, 2);

            buffer.push(node);
        });

        return buffer;
    }

    createBufferByRootAction(moduleActions, action) {
        var buffer = [];

        var node = {
            Action: action,
            CompletedActions: [],
            SuccessActions: [],
            ErrorActions: [],
        };

        this.getChildActions(node.CompletedActions, moduleActions, action.ActionId, 0);
        this.getChildActions(node.SuccessActions, moduleActions, action.ActionId, 1);
        this.getChildActions(node.ErrorActions, moduleActions, action.ActionId, 2);

        buffer.push(node);

        return buffer;
    }

    getChildActions(buffer, moduleActions, parentId, parentResultStatus) {
        const actions = _.filter(moduleActions, (action) => {
            return (
                action.Event == "OnActionCompleted" &&
                action.ParentId == parentId &&
                action.ParentResultStatus == parentResultStatus
            );
        });

        _.forEach(actions, (action) => {
            var node = {
                Action: action,
                CompletedActions: [],
                SuccessActions: [],
                ErrorActions: [],
            };

            this.getChildActions(
                node.CompletedActions,
                moduleActions,
                action.ActionId,
                0
            );
            this.getChildActions(node.SuccessActions, moduleActions, action.ActionId, 1);
            this.getChildActions(node.ErrorActions, moduleActions, action.ActionId, 2);

            buffer.push(node);
        });

        return buffer;
    }

    callActionFromBuffer(buffer, defer, data) {
        if (!buffer || !buffer.length) {
            defer.resolve();
            return defer.promise;
        }

        var node = buffer[0];
        var action = node.Action;

        this.callAction(action, data).then((data) => {
            if (data && data.invalidConditions) {
                defer.resolve();
                return defer.promise;
            }
            if (node.SuccessActions.length)
                this.callActionFromBuffer(node.SuccessActions, defer, data);
            else
                defer.resolve(data);
        }, (error) => {
            if (node.ErrorActions.length)
                this.callActionFromBuffer(node.ErrorActions, defer, data);
            else
                defer.reject(error);
        }).finally((data) => {
            if (node.CompletedActions.length)
                this.callActionFromBuffer(node.CompletedActions, defer, data);
            else
                defer.resolve(data);

            buffer.shift();
            this.callActionFromBuffer(buffer, defer, data);
        });

        return defer.promise;
    }

    callAction(action, data) {
        const defer = this.$q.defer();

        var actionMethod = action.ExecuteInClientSide
            ? this.callClientAction
            : this.callServerAction;

        var params = _.cloneDeep(action.Params);

        //proccess action conditions
        const isValid = this.expressionService.checkConditions(action.Conditions, data);
        if (isValid) {
            if (action.ExecuteInClientSide) {
                //proccess and set the action params for example one row in the condition list is "_CurrentUser:UserId > 0"
                this.proccessActionParams(params, data);
            }

            // call the action with filled the action params
            actionMethod.apply(this, [data, action, params]).then((data) => {
                defer.resolve(data);
            });
        }
        else {
            defer.resolve({ invalidConditions: true });
        }

        return defer.promise;
    }

    callClientAction(data, action, params) {
        const defer = this.$q.defer();

        try {
            const actionFunction = eval(`${action.ActionType}ActionController`);
            const actionController = new actionFunction(this, data);
            if (typeof eval("actionController.execute") == "function") {
                actionController.execute(action, params, defer).then((data) => {
                    //after running the action and now fill variables with the action results 
                    this.setActionResults(action, data, data);

                    defer.resolve(data);
                }, (error) => {
                    this.setActionResults(action, error, data);
                    defer.reject(error.data);
                })
            } else {
                throw new Error('');
            }
        } catch (error) {
            console.error('the action controller not found!.');

            defer.reject({ error: 'the action controller not found!.' });
        }

        return defer.promise;
    }

    callServerAction(data, action) {
        const defer = this.$q.defer();

        this.apiService.post("Module", "CallAction", {
            ActionId: action.ActionId,
            ModuleId: action.ModuleId,
            ConnectionId: data.connectionId,
            Form: data.Form,
            Field: data.Field,
            PageUrl: document.URL,
        }).then((data) => {
            if (data) {
                this.globalService.parseJsonItems(data);

                this.assignScopeData(data, data);
            }

            defer.resolve(data);
        },
            (error) => {
                defer.reject(error.data);
            }
        );

        return defer.promise;
    }

    assignScopeData(serverObjects, data) {
        _.forOwn(serverObjects, (value, key) => {
            if (key == 'Field') {
                //این قسمت باید بازنویسی شود و با فعال کردن کدهای زیر آبجکت فیلد دچار مشکل می شود
                // _.forOwn(value, (fieldData, fieldName) => {
                //     data.Field[fieldName] = {...data.Field[fieldName], ...fieldData };
                //     _.filter(data.moduleController.fields, (f) => { return f.FieldName == fieldName }).map((f) => {
                //         data.moduleController.fields[data.moduleController.fields.indexOf(f)] = data.Field[fieldName]
                //     });
                // });
            } else {
                if (data[key])
                    value && typeof value == 'object' ? data[key] = { ...data[key], ...value } : value;
                else
                    data[key] = value;
            }
        });
    }

    proccessActionParams(actionParams, data) {
        _.forEach(actionParams, (item) => {
            item.ParamValue = this.expressionService.parseExpression(
                item.ParamValue,
                data
            );
        });
    }

    setActionResults(action, data) {
        _.forEach(action.Results, (item) => {
            const value = this.processActionResultsToken(
                item.RightExpression,
                data
            );

            this.expressionService.setVariableValue(
                item.LeftExpression,
                data,
                value
            );
        });
    }

    processActionResultsToken(expression, data) {
        var result;

        const match = /(?:_ServiceResult)\.?(.[^{}:\$,]+)?$/.exec(expression);
        if (match) {
            if (expression == match[0] && !match[1]) result = data;
            else result = _.get(data, match[1]);
        } else result = this.expressionService.parseExpression(expression, data);

        return result;
    }

    runPrescript(action, params, data) {
        const defer = this.$q.defer();

        if (action.HasPreScript)
            return this.runScript(
                data,
                action,
                params,
                action.PreScript,
                "Prescript_"
            );
        else defer.resolve();

        return defer.promise;
    }

    runPostscript(action, params, data) {
        const defer = this.$q.defer();

        if (action.HasPostScript)
            return this.runScript(
                data,
                action,
                params,
                action.PostScript,
                "Postscript_"
            );
        else defer.resolve();

        return defer.promise;
    }

    runScript(data, action, params, actionScript, type) {
        const defer = this.$q.defer();
        const actionName = type + action.ActionName;

        actionScript = actionScript || '';

        var scriptStr = `function ${actionName}ControllerAction(data, moduleController,action,_ActionParam) { 
            const defer = moduleController.$q.defer(); 
            
            try { 
                ${actionScript}

                //{SCRIPT-DONE} 
            } catch (e) { 
                defer.resolve(0,e); 
            } 

            return defer.promise; 
        }`;

        if (actionScript.indexOf("//{SCRIPT-DONE}") >= 0)
            scriptStr = scriptStr.replace("//{SCRIPT-DONE}", "");
        scriptStr = scriptStr.replace("//{SCRIPT-DONE}", "defer.resolve()");

        var newScript = document.createElement("script");
        newScript.innerHTML = scriptStr;
        $("body").append(newScript);

        var actionParam = {};
        _.map(action.Params, (param) => {
            var filledParam = _.find(params, (p) => {
                return p.ParamName == param.ParamName;
            });
            actionParam[param.ParamName] = !filledParam ?
                filledParam.ParamValue :
                param.ParamValue;
        });

        const actionFunction = eval(`${actionName}ControllerAction`);
        new actionFunction(
            data,
            data.moduleController,
            action,
            actionParam
        ).then((data) => {
            defer.resolve();
        });

        return defer.promise;
    }
}