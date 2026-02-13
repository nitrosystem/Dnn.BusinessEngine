export class ActionService {
    constructor(
        globalService,
        apiService,
        expressionService
    ) {
        this.globalService = globalService;
        this.apiService = apiService;
        this.expressionService = expressionService;

        this.controllerCache = {};
    }

    async callActionsByEvent(controller, event, actions, extraParams) {
        const nodes = this.buildActionTree(actions, event);
        return await this.executeActionTree(controller, nodes, 0, extraParams);
    }

    async callActions(controller, actionId, actions, extraParams) {
        const nodes = this.buildActionTreeByParent(actionId, actions);
        return await this.executeActionTree(controller, nodes, 0, extraParams);
    }

    async executeActionTree(controller, nodes, parentResultStatus, extraParams, executedServerIds = new Set()) {
        let index = 0;
        const results = [];
        for (const node of nodes ?? []) {
            if (!node.ExecuteInClientSide && executedServerIds.has(node.Id))
                continue;

            if (node.ParentId && node.ParentActionTriggerCondition && node.ParentActionTriggerCondition !== parentResultStatus)
                continue;

            if (node.ExecuteInClientSide) {
                const result = await this.callClientAction(controller, node);
                results.push(result);

                await this.executeActionTree(controller, node.children, result.status, extraParams, executedServerIds);
            }
            else {
                const serverActionsId = this.collectServerSideActions(controller, node);
                serverActionsId.forEach(id => executedServerIds.add(id));

                const result = await this.callServerActions(controller, serverActionsId, extraParams);
                results.push(result);

                await this.executeActionTree(controller, node.children, result.status, extraParams, executedServerIds);

                break;
            }

            index++;
        }

        return results;
    }

    async callClientAction(controller, action) {
        const isTrue = action.Conditions
            ? this.expressionService.evaluateExpression(action.Conditions, controller.scope)
            : true;

        if (typeof isTrue == 'string') isTrue = JSON.parse(value);

        if (!isTrue)
            return 3;

        let scopeInstance = this.controllerCache[action.ActionType];
        if (!scopeInstance) {
            const ControllerClass = ActionRegistry.resolve(action.ActionType);
            if (typeof ControllerClass === 'function') {
                scopeInstance = new ControllerClass(controller.scope, this);
                this.controllerCache[action.ActionType] = scopeInstance;
            }
        }

        if (scopeInstance && typeof scopeInstance.execute === 'function') {
            try {
                return await scopeInstance.execute(action)
            } catch (error) {
                console.error(error);
                return await 2;
            }
        }
    }

    async callServerActions(controller, actionIds, extraParams) {
        const result = { status: 1 };
        const scope = controller.scope;

        const postData = {};
        for (const key in scope) {
            const variable = controller.variables.find(v => v.VariableName === key);
            if (!variable || variable.Scope == 1) continue;

            postData[key] = scope[key];
        }

        const responses = await this.apiService.postAsync("Module", "CallAction", {
            ConnectionId: controller.connectionId,
            ModuleId: controller.moduleId,
            Data: postData,
            PageUrl: document.URL,
            ExtraParams: extraParams,
            ActionIds: actionIds,
        });

        for (const response of responses ?? []) {
            if (response.Status === 2 && response.ErrorException) {
                result.status = 2;
                result.isError = true;
                result.error = response.ErrorException;
                console.error(response.ErrorException)
                break;
            }

            if (!response.IsRequiredToUpdateData) continue;

            const moduleData = response.ModuleData;
            for (const key in moduleData ?? {}) {
                const newValue = moduleData[key];
                if (scope[key] !== newValue) {
                    const variable = controller.variables.find(v => v.VariableName === key);
                    if (scope[key] && variable && variable.VariableType === 'AppModel') {
                        for (const prop of variable.Properties) {
                            scope[key][prop.PropertyName] = moduleData[key][prop.PropertyName];
                            controller.notifyResolved(scope[key], prop.PropertyName);
                        }
                    }
                    else
                        controller.set(key, newValue, true);
                }
            }
        }

        if (!result.isError) result.isSuccess = true;
        return result;
    }

    buildActionTree(actions, event) {
        const clonedActions = this.globalService.cloneDeep(actions);
        const rootActions = clonedActions.filter(a => { return !a.ParentId && a.Event === event });
        const lookup = this.globalService.keyBy(clonedActions, 'Id');

        for (const action of clonedActions.filter(a => { return !!a.ParentId })) {
            lookup[action.ParentId].children = lookup[action.ParentId].children ?? [];
            lookup[action.ParentId].children.push(lookup[action.Id]);
        }

        const sortTree = nodes => {
            nodes.sort((a, b) => a.ViewOrder - b.ViewOrder);
            nodes.forEach(n => {
                if (n.children && n.children.length) sortTree(n.children)
            });
        };

        sortTree(rootActions);
        return rootActions;
    }

    buildActionTreeByParent(parentActionId, actions) {
        const clonedActions = this.globalService.cloneDeep(actions);
        const rootActions = clonedActions.filter(a => { return a.Id === parentActionId });
        const lookup = this.globalService.keyBy(clonedActions, 'Id');

        for (const action of clonedActions.filter(a => { return !!a.ParentId })) {
            lookup[action.ParentId].children = lookup[action.ParentId].children ?? [];
            lookup[action.ParentId].children.push(lookup[action.Id]);
        }

        const sortTree = nodes => {
            nodes.sort((a, b) => a.ViewOrder - b.ViewOrder);
            nodes.forEach(n => {
                if (n.children && n.children.length) sortTree(n.children)
            });
        };

        sortTree(rootActions);
        return rootActions;
    }

    collectServerSideActions(controller, node) {
        const result = [];

        const dfs = n => {
            const isTrue = !n.ParentId && n.Preconditions ?
                this.expressionService.evaluateExpression(n.Preconditions, controller.scope)
                : true;

            if (typeof isTrue == 'string') isTrue = JSON.parse(value);

            if (isTrue) {
                if (!n.ExecuteInClientSide)
                    result.push(n.Id);

                if (n.children && n.children.length)
                    n.children.forEach(child => dfs(child));
            }
        };

        dfs(node);
        return result;
    }
}