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
        const id = this.globalService.getParameterByName('id');

        this.running = "get-actions";
        this.awaitAction = {
            title: "Loading Actions",
            subtitle: "Just a moment for loading actions...",
        };

        this.apiService.get("Module", "GetActions", {
            moduleId: id,
            pageIndex: this.filter.pageIndex,
            pageSize: this.filter.pageSize,
            fieldId: this.filter.fieldId,
            searchText: this.filter.searchText,
            actionType: this.filter.actionType,
            sortBy: this.filter.sortBy
        }).then((data) => {
            this.actions = data.Actions;
            this.fields = data.Fields;
            this.module = { Id: id }

            debugger
            var items = {};
            var groups = _.groupBy(_.filter(this.actions, (a) => { return a.FieldId }), 'FieldName');
            for (var key in groups) {
                items[key] = this.populateActions(groups[key]);
            }
            this.fieldActions = items;

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
        const actionMap = new Map();
        const roots = [];

        // آماده‌سازی: map کردن به ازای id
        actions.forEach(action => {
            action.Childs = [];
            actionMap.set(action.Id, action);
        });

        // رابطه پدر-فرزندی
        actions.forEach(action => {
            if (action.ParentId) {
                const parent = actionMap.get(action.ParentId);
                if (parent) {
                    parent.Childs.push(action);
                }
            } else {
                roots.push(action);
            }
        });

        // علامت‌گذاری first/last بر اساس ViewOrder
        const markFirstLast = (items) => {
            const sorted = [...items].sort((a, b) => (a.ViewOrder ?? 0) - (b.ViewOrder ?? 0));
            sorted.forEach((item, index) => {
                item.IsFirst = index === 0;
                item.IsLast = index === sorted.length - 1;
                if (item.Childs?.length) {
                    markFirstLast(item.Childs);
                }
            });
        };

        markFirstLast(roots);

        return roots;
    }

    validateStep(task, args) {
        task.wait(() => {
            const $defer = this.$q.defer();

            $defer.resolve(true);

            return $defer.promise;
        });
    }

    onAddActionClick(fieldType, fieldId) {
        const page = {
            page: "create-action",
            subParams: {
                module: this.module.Id,
                ...(fieldType && { type: fieldType }),
                ...(fieldId && { field: fieldId }),
            }
        };

        this.$scope.$emit("onGotoPage", page);
    }

    onEditActionClick(actionId, fieldType, fieldId) {
        debugger
        const page = {
            page: "create-action",
            id: actionId,
            subParams: {
                module: this.module.Id,
                ...(fieldType && { type: fieldType }),
                ...(fieldId && { field: fieldId }),
            }
        };

        this.$scope.$emit("onGotoPage", page);
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

                this.apiService.post("Studio", "DeleteAction", { Id: id }).then((data) => {
                    this.actions.splice(index, 1);

                    this.notifyService.success("Action deleted has been successfully");

                    delete this.awaitAction;
                    delete this.running;
                }, (error) => {
                    this.awaitAction.isError = true;
                    this.awaitAction.subtitle = error.statusText;
                    this.awaitAction.desc =
                        this.globalService.getErrorHtmlFormat(error);

                    this.notifyService.error(error.data.Message);

                    delete this.running;
                });
            }
        });
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}