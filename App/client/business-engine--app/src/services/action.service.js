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

    async executeActionTree(controller, nodes, parentStatus, executedServerIds = new Set()) {
        let index = 0;
        for (const node of nodes ?? []) {
            if (!node.ExecuteInClientSide && executedServerIds.has(node.Id))
                continue;

            if (node.ParentId && node.ParentActionTriggerCondition && node.ParentActionTriggerCondition != parentStatus)
                continue;

            if (node.ExecuteInClientSide) {
                var status = await this.callClientAction(controller, node);
                await this.executeActionTree(controller, node.children, status, executedServerIds);
            }
            else {
                const serverActionsId = this.collectServerSideActions(controller, node);
                serverActionsId.forEach(id => executedServerIds.add(id));

                const status = await this.callServerActions(controller, serverActionsId);
                await this.executeActionTree(controller, node.children, status, executedServerIds);

                break;
            }

            index++;
        }
    }

    collectServerSideActions(controller, node) {
        const result = [];

        const dfs = n => {
            if (!n.ExecuteInClientSide && this.expressionService.checkConditions(controller.Data, n.Conditions)) {
                result.push(n.Id);
                if (n.children && n.children.length) n.children.forEach(child => dfs(child));
            }
        };

        dfs(node);
        return result;
    }

    async callActions(controller, actions, event) {
        var nodes = this.buildActionTree(actions, event);
        await this.executeActionTree(controller, nodes, 0);
    }

    async callClientAction(action, controller) {
        if (!this.expressionService.checkConditions(action.Conditions, controller.Data))
            return await 3;

        let controllerInstance = this.controllerCache[action.ActionType];
        if (!controllerInstance) {
            const ControllerClass = ActionRegistry.resolve(action.ActionType);
            if (typeof ControllerClass === 'function') {
                controllerInstance = new ControllerClass(controller, this);
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

    async callServerActions(controller, actionIds) {
        try {
            const data = await this.apiService.post("Module", "CallAction", {
                ConnectionId: controller.connectionId,
                ModuleId: controller.moduleId,
                Data: controller.Data,
                PageUrl: document.URL,
                ActionIds: actionIds,
            });

            controller.Data = _.merge(_.cloneDeep(controller.Data), data);

            return await 1;
        }
        catch (error) {
            console.error(error);
            return await 2;
        }
    }
}