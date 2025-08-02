export class ActionService {
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

    buildActionTree(actions, event) {
        const rootActions = _.filter(actions, (a) => { return !a.ParentId && a.Event === event });
        const lookup = _.keyBy(actions, 'Id');

        for (const action of _.filter(actions, (a) => { return !!a.ParentId })) {
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
        for (const node of nodes) {
            if (!node.ExecuteInClientSide && executedServerIds.has(node.id))
                continue;

            if (!this.expressionService.checkConditions(
                _.filter(node.Conditions, (c) => { return node.ExecuteInClientSide || c.EvaluateInClient }),
                moduleController.data)
            )
                continue;

            if (node.ParentId && node.ParentActionTriggerCondition != parentStatus)
                continue;

            if (node.ExecuteInClientSide) {
                var result = await this.executeClientAction(node, moduleController);
                await this.executeActionTree(node.children, result.status, moduleController, executedServerIds);
            }
            else {
                const serverActionsId = this.collectServerSideActions(node);
                serverActionsId.forEach(id => executedServerIds.add(id));

                const result = await this.callServerAction(serverActionsId, moduleController);
                await this.executeActionTree(node.children, result.status, moduleController, executedServerIds);

                break;
            }

            index++;
        }
    }

    collectServerSideActions(node) {
        const result = [];

        const dfs = n => {
            if (!n.ExecuteInClientSide) {
                result.push(n.Id);
                if (n.children && n.children.length) n.children.forEach(child => dfs(child));
            }
        };

        dfs(node);
        return result;
    }

    collectServerSideActionsTemp(nodes, index, result) {
        const node = nodes[index];

        const dfs = n => {
            if (!n.ExecuteInClientSide) {
                result.push(n.Id);
                if (n.children.length) this.collectServerSideActions(n.children, 0, result);
            }
        };

        dfs(node);

        index++;

        if (nodes.length > index) this.collectServerSideActions(nodes, index, result);

        return result;
    }

    async callActions(actions, event, moduleController) {
        var nodes = this.buildActionTree(actions, event);
        this.executeActionTree(nodes, 0, moduleController);
    }

    async callClientAction(action, moduleController) {
        debugger
        const actionFunction = eval(`${action.ActionType}ActionController`);
        const actionController = new actionFunction(this, data);
        await actionController.execute(action).then((data) => {
            //this.setActionResults(action, data, data);

            return 1;
        }, (error) => {
            console.error(error);
            //this.setActionResults(action, error, data);
            return 2;
        });
    }

    async callServerAction(actionIds, moduleController) {
        await this.apiService.post("Module", "CallAction", {
            ConnectionId: moduleController.module.connectionId,
            ModuleId: moduleController.module.moduleId,
            Data: moduleController.data,
            PageUrl: document.URL,
            ActionIds: actionIds,
        }).then((data) => {
            moduleController.data = _.merge(_.cloneDeep(moduleController.data), data);
            return 1;
        }, (error) => {
            console.error(error);
            return 2;
        });
    }
}