import Swal from 'sweetalert2'
import { GlobalUtil } from '../../../shared/utils';
import { UrlHelper } from '../../../shared/helpers';

export class CreateModuleActionsController {
    constructor($scope, $rootScope, $q,  apiService, notificationService) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$q = $q;
        this.apiService = apiService;
        this.notifyService = notificationService;

        this.filter = {};

        $rootScope.$on('onUpdateActions', (e, args) => {
            if (args.moduleId === this.module.Id)
                this._getActions(args.moduleId);
        });

        $scope.$on("onCreateModuleValidateStep6", (e, task, args) => {
            this.validateStep.apply(this, [task, args]);
        });

        this.$scope.$parent.createModuleValidatedStep.push(6);
        $scope.$on("onCreateModuleGotoStep6", (e, defer) => {
            defer.resolve();
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        this._getActions();
    }

    onTableModeClick() {
        this.displayMode = 'table';
    }

    onBoxModeClick() {
        this.displayMode = 'box';
    }

    onApplyFilter() {
        this.entities = null;
        this.filter.filtered = true;
        this._getActions();
    }

    onClearFilterClick() {
        this.filter = {};
        this._getActions();
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
        let timerInterval;
        Swal.fire({
            title: 'Are you sure?',
            html: '<p>Once deleted, you will not be able to recover this imaginary entity!</p><b></b>',
            icon: "warning",
            timer: 5000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!",
            backdrop: false,
            didOpen: () => {
                const timer = Swal.getPopup().querySelector("b");
                timerInterval = setInterval(() => {
                    timer.textContent = `${Swal.getTimerLeft()}`;
                }, 100);
            },
            willClose: () => {
                clearInterval(timerInterval);
            }
        }).then((result) => {
            if (result.isConfirmed) {
                this.running = "remove-action";
                this.awaitAction = {
                    title: "Remove Action",
                    subtitle: "Just a moment for removing action...",
                };

                this.apiService.post("Module", "DeleteAction", { Id: id }).then((data) => {
                    if (data) {
                        this.notifyService.info("Action has been deleted successfully.");
                        this._getActions(this.module.Id);
                    }

                    this.awaitAction = null;
                    this.running = null;
                }, (error) => {
                    this.awaitAction.isError = true;
                    this.awaitAction.subtitle = error.statusText;
                    this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                    this.notifyService.error(error.data);

                    this.running = null;
                });
            }
        });
    }

    onPreviousStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 5 });
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#endregion

    //#region Private Methods

    _getActions(moduleId) {
        moduleId = moduleId ?? UrlHelper.getParameterByName('id');

        this.running = "get-actions";
        this.awaitAction = {
            title: "Loading Actions",
            subtitle: "Just a moment for loading actions...",
        };

        this.apiService.get("Module", "GetActions", {
            moduleId: moduleId,
            fieldId: this.filter.fieldId,
            searchText: this.filter.searchText,
            actionType: this.filter.actionType
        }).then((data) => {
            this.module = data.Module;
            this.actionTypes = data.ActionTypes;
            this.actions = data.Actions;
            this.fields = data.Fields;

            const moduleActions = this.actions.filter(a => a.FieldId === null);
            const nonNullActions = this.actions.filter(a => a.FieldId !== null);
            this.moduleActions = moduleActions;
            const grouped = {};
            for (const item of nonNullActions) {
                (grouped[item.FieldName] ||= []).push(item);
            }
            const fieldActions = Object.keys(grouped)
                .sort()
                .reduce((acc, key) => {
                    acc[key] = this._populateActions(grouped[key]);
                    return acc;
                }, {});


            this.moduleActions = moduleActions;
            this.fieldActions = fieldActions;

            this.running = null;
            this.awaitAction = null;
        });
    }

    _populateActions(actions) {
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

        // علامت‌گذاری first/last بر اساس ExecuteOrder
        const markFirstLast = (items) => {
            const sorted = [...items].sort((a, b) => (a.ExecuteOrder ?? 0) - (b.ExecuteOrder ?? 0));
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

    //#endregion
}