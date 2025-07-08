import { GlobalSettings } from "../../angular-configs/global.settings";

export class CreateModuleActionsController {
    constructor($scope, $rootScope, $q, globalService, apiService, studioService, notificationService) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$q = $q;
        this.globalService = globalService;
        this.apiService = apiService;
        this.studioService = studioService;
        this.notifyService = notificationService;

        this.filter = { pageIndex: 1, pageSize: 10 };

        this.$rootScope.createModuleValidatedStep.push(6);

        $scope.$on("onCreateModuleValidateStep6", (e, task, args) => {
            this.validateStep.apply(this, [task, args]);
        });

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        this.running = "get-actions";
        this.awaitAction = {
            title: "Loading Actions",
            subtitle: "Just a moment for loading actions...",
        };

        let moduleId = this.globalService.getParameterByName('id');

        this.apiService.get("Module", "GetActions", {
            moduleId: moduleId,
            pageIndex: this.filter.pageIndex,
            pageSize: this.filter.pageSize,
            fieldId: this.filter.fieldId,
            searchText: this.filter.searchText,
            actionType: this.filter.actionType,
            sortBy: this.filter.sortBy
        }).then((data) => {
            this.actions = data.Actions;
            this.fields = data.Fields;

            if (!this.filter.fieldId) {
                var items = {};
                var groups = _.groupBy(_.filter(this.actions, (a) => { return a.FieldId }), 'FieldName');
                for (var key in groups) {
                    items[key] = this.populateActions(groups[key]);
                }
                this.fieldActions = items;
            }
            //this.onFocusModule();

            delete this.running;
            delete this.awaitAction;
        });
    }

    onFocusModule() {
        const items = this.studioService.createSidebarExplorerPath(this.module.Id, "Module");
        this.$rootScope.explorerExpandedItems.push(...items);
        this.$rootScope.explorerExpandedItems.push(this.module.ModuleType.toLowerCase() + '-modules');

        this.$rootScope.explorerCurrentItem = this.module.Id;
    }

    onTableModeClick() {
        this.displayMode = 'table';
    }

    onBoxModeClick() {
        this.displayMode = 'box';
    }

    populateActions(actions) {
        var result = [];

        const findChildActions = (parent) => {
            parent.Childs = [];
            let childActions = _.filter(actions, (a) => { return a.ParentId == parent.ActionId; });
            let index = 0;
            _.forEach(_.sortBy(childActions, ["ViewOrder"]), (action) => {
                if (index == 0) action.IsFirst = true;
                if (index == childActions.length - 1) action.IsLast = true;

                parent.Childs.push(action);

                index++;

                findChildActions(action);
            });
        }

        let parentActions = _.filter(actions, (a) => { return !a.ParentId; });
        let index = 0;
        _.forEach(_.sortBy(parentActions, ["ViewOrder"]), (action) => {
            if (index == 0) action.IsFirst = true;
            if (index == parentActions.length - 1) action.IsLast = true;

            result.push(action);

            index++;

            findChildActions(action);
        });

        return result;
    }

    validateStep(task, args) {
        task.wait(() => {
            const $defer = this.$q.defer();

            $defer.resolve(true);

            return $defer.promise;
        });
    }

    onAddActionClick() {
        var subParams = {};
        if (this.isFieldActions) subParams.type = "field";

        this.$scope.$emit("onGotoPage", {
            page: "create-action",
            parentId: this.parentId,
            subParams: subParams,
        });
    }

    onEditActionClick(id, title, fieldId) {
        var subParams = {};
        if (fieldId) subParams.type = "field";

        this.$scope.$emit("onGotoPage", {
            page: "create-action",
            parentId: fieldId || this.module.Id,
            id: id,
            title: title,
            subParams: subParams,
        });
    }

    onEditServiceClick(serviceId) {
        this.$scope.$emit("onGotoPage", {
            page: 'create-service',
            id: serviceId
        });
    }

    onDeleteActionClick(id, index) {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this imaginary action!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((willDelete) => {
            if (willDelete) {
                this.running = "remove-action";
                this.awaitAction = {
                    title: "Remove Action",
                    subtitle: "Just a moment for removing action...",
                };

                this.apiService.post("Studio", "DeleteAction", { Id: id }).then(
                    (data) => {
                        this.allActions.splice(index, 1);

                        this.notifyService.success("Action deleted has been successfully");

                        delete this.awaitAction;
                        delete this.running;
                    },
                    (error) => {
                        this.awaitAction.isError = true;
                        this.awaitAction.subtitle = error.statusText;
                        this.awaitAction.desc =
                            this.globalService.getErrorHtmlFormat(error);

                        this.notifyService.error(error.data.Message);

                        delete this.running;
                    }
                );
            }
        });
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}