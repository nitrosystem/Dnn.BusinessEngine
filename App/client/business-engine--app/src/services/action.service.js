export class ActionService {
    constructor(
        $timeout,
        $q,
        $window,
        globalService,
        apiService,
        expressionService
    ) {
        this.$timeout = $timeout;
        this.$q = $q;
        this.$window = $window;
        this.globalService = globalService;
        this.apiService = apiService;
        this.expressionService = expressionService;

        this.controllerCache = {};
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

    async executeActionTree(nodes, parentStatus, moduleController, executedServerIds = new Set()) {
        let index = 0;
        for (const node of nodes ?? []) {
            if (!node.ExecuteInClientSide && executedServerIds.has(node.Id))
                continue;

            if (node.ParentId && node.ParentActionTriggerCondition && node.ParentActionTriggerCondition != parentStatus)
                continue;

            if (node.ExecuteInClientSide) {
                var status = await this.callClientAction(node, moduleController);
                await this.executeActionTree(node.children, status, moduleController, executedServerIds);
            }
            else {
                const serverActionsId = this.collectServerSideActions(node, moduleController);
                serverActionsId.forEach(id => executedServerIds.add(id));

                const status = await this.callServerActions(serverActionsId, moduleController);
                await this.executeActionTree(node.children, status, moduleController, executedServerIds);

                break;
            }

            index++;
        }
    }

    collectServerSideActions(node, moduleController) {
        const result = [];

        const dfs = n => {
            if (!n.ExecuteInClientSide && this.expressionService.checkConditions(n.Conditions, moduleController.data)) {
                result.push(n.Id);
                if (n.children && n.children.length) n.children.forEach(child => dfs(child));
            }
        };

        dfs(node);
        return result;
    }

    async callActions(actions, event, moduleController) {
        var nodes = this.buildActionTree(actions, event);
        await this.executeActionTree(nodes, 0, moduleController);
    }

    async callClientAction(action, moduleController) {
        if (!this.expressionService.checkConditions(action.Conditions, moduleController.data))
            return await 3;

        let controllerInstance = this.controllerCache[action.ActionType];
        if (!controllerInstance) {
            const ControllerClass = ActionRegistry.resolve(action.ActionType);
            if (typeof ControllerClass === 'function') {
                controllerInstance = new ControllerClass(moduleController, this);
                this.controllerCache[action.ActionType] = controllerInstance;
            }
        }

        if (controllerInstance && typeof controllerInstance.execute === 'function') {
            try {
                return await controllerInstance.execute(action)
            } catch (error) {
                console.error(error);
                return await 2;
            }
        }
    }

    async callServerActions(actionIds, moduleController) {
        try {
            const data = await this.apiService.post("Module", "CallAction", {
                ConnectionId: moduleController.module.connectionId,
                ModuleId: moduleController.module.moduleId,
                Data: moduleController.data,
                PageUrl: document.URL,
                ActionIds: actionIds,
            });

            moduleController.data = _.merge(_.cloneDeep(moduleController.data), data);

            return await 1;
        }
        catch (error) {
            console.error(error);
            return await 2;
        }
    }
}