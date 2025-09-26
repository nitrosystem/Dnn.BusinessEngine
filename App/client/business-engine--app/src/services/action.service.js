export class ActionService {
    constructor(
        globalService,
        apiService,
        expressionService
    ) {
        this.globalService = globalService;
        this.apiService = apiService;
        this.expressionService = expressionService;

        this.scopeCache = {};
    }

    async callActions(event, actions, scope) {
        const nodes = this.buildActionTree(actions, event);

        await this.executeActionTree(nodes, 0, scope);
    }

    async executeActionTree(nodes, parentStatus, scope, executedServerIds = new Set()) {
        let index = 0;
        for (const node of nodes ?? []) {
            if (!node.ExecuteInClientSide && executedServerIds.has(node.Id))
                continue;

            if (node.ParentId && node.ParentActionTriggerCondition && node.ParentActionTriggerCondition != parentStatus)
                continue;

            if (node.ExecuteInClientSide) {
                const status = await this.callClientAction(node, scope);

                await this.executeActionTree(node.children, status, scope, executedServerIds);
            }
            else {
                const serverActionsId = this.collectServerSideActions(node, scope);
                serverActionsId.forEach(id => executedServerIds.add(id));

                const status = await this.callServerActions(serverActionsId, scope);
                await this.executeActionTree(node.children, status, scope, executedServerIds);

                break;
            }

            index++;
        }
    }

    async callClientAction(action, scope) {
        const isTrue = expressionService.evaluateExpression(action.Conditions, scope);
        if (typeof isTrue == 'string') isTrue = JSON.parse(value);

        if (!isTrue)
            return 3;

        let scopeInstance = this.scopeCache[action.ActionType];
        if (!scopeInstance) {
            const ControllerClass = ActionRegistry.resolve(action.ActionType);
            if (typeof ControllerClass === 'function') {
                scopeInstance = new ControllerClass(scope, this);
                this.scopeCache[action.ActionType] = scopeInstance;
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

    async callServerActions(actionIds, scope) {
        try {
            const module = await this.apiService.postAsync("Module", "CallAction", {
                ConnectionId: scope.connectionId,
                ModuleId: scope.moduleId,
                Data: scope,
                PageUrl: document.URL,
                ActionIds: actionIds,
            });

            for (const key in module.data) {
                const variable = scope.variables.find(v => v.VariableName === key);

                if (!variable) continue;

                if (variable.VariableType == 'AppModel' && !module.data[key]) {
                    scope[key] = {};
                    variable.Properties.forEach(prop => {
                        scope[key][prop.PropertyName] = this.globalService.getDefaultValueByType(undefined, prop.PropertyType);
                    });
                }
                else if (this.globalService.isSystemType(variable.VariableType))
                    scope[key] = this.globalService.convertToRealType(module.data[key], variable.VariableType);
                else
                    scope[key] = module.data[key];
            }

            return 1;
        }
        catch (error) {
            console.error(error);
            return 2;
        }
    }

    buildActionTree(actions, event) {
        const clonedActions = _.cloneDeep(actions);
        const rootActions = _.filter(clonedActions, (a) => { return !a.ParentId && a.Event === event });
        const lookup = _.keyBy(clonedActions, 'Id');

        for (const action of _.filter(clonedActions, (a) => { return !!a.ParentId })) {
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

    collectServerSideActions(node, scope) {
        const result = [];

        const dfs = n => {
            const isTrue = !n.ParentId && n.Preconditions ?
                expressionService.evaluateExpression(n.Preconditions, scope)
                : true;

            if (typeof isTrue == 'string') isTrue = JSON.parse(value);

            if (isTrue) {
                result.push(n.Id);
                if (n.children && n.children.length) n.children.forEach(child => dfs(child));
            }
        };

        dfs(node);
        return result;
    }
}