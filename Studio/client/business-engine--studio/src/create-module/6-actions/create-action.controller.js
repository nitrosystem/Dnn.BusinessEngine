import { GlobalSettings } from "../../angular-configs/global.settings";

export class CreateModuleCreateActionController {
    constructor(
        $scope,
        $rootScope,
        $compile,
        $timeout,
        $q,
        globalService,
        apiService,
        studioService,
        validationService,
        notificationService,
        $deferredBroadcast
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$compile = $compile;
        this.$timeout = $timeout;
        this.$q = $q;
        this.globalService = globalService;
        this.apiService = apiService;
        this.studioService = studioService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.$deferredBroadcast = $deferredBroadcast;

        this.stepsCallback = { 3: this.initActionBuilder };
        this.actionBuilder = {};
        this.events = [];

        $scope.clientSideFilter = (item) => {
            return !item.ExecutionScope || item.ExecutionScope == (this.action.ExecuteInClientSide ? 1 : 2);
        };

        $scope.$on("onSyncActionParamsWithServiceParams", (e, args) => {
            const service = args.service || {};
            const action = args.action || {};

            action.Params = action.Params || [];
            _.forEach(service.Params, (sp) => {
                if (!_.filter(action.Params, (ap) => {
                    return ap.ParamName == sp.ParamName;
                }).length)
                    action.Params.push({ ParamName: sp.ParamName });
            });
        });

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        const moduleId = this.globalService.getParameterByName("module");
        const fieldId = this.globalService.getParameterByName("field");
        const fieldType = this.globalService.getParameterByName("type");
        const actionId = this.globalService.getParameterByName("id");

        var step = this.globalService.getParameterByName("st");
        this.step = step && actionId ? parseInt(step) : 1;
        this.stepsValid = 1;

        this.running = "get-action";
        this.awaitAction = {
            title: "Loading Action",
            subtitle: "Just a moment for loading action...",
        };

        this.apiService.get("Module", "GetAction", {
            moduleId: moduleId,
            fieldId: fieldId,
            actionId: actionId,
            fieldType: fieldType,
        }).then((data) => {
            //this.module = data.Module;
            this.actionTypes = data.ActionTypes;
            this.actions = data.Actions;
            this.variables = data.Variables;
            this.events = data.Events;
            this.action = data.Action;

            if (!this.action) {
                this.isNewAction = true;
                this.action = {
                    ModuleId: moduleId,
                    FieldId: fieldId,
                    Options: {},
                };

                this.step = 1;
            } else {
                if (!step) this.step = 3;

                this.stepsValid = 4;

                _.filter(this.actionTypes, (st) => {
                    return st.ActionType == this.action.ActionType;
                }).map((actionType) => {
                    this.actionType = actionType;
                });

                this.gotoStep(this.step);

                this.$scope.$emit("onUpdateCurrentTab", {
                    id: this.action.Id,
                    title: this.action.ActionName,
                });
            }

            this.onFocusModule();
            this.setForm();

            delete this.running;
            delete this.awaitAction;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });
    }

    onFocusModule() {
        if (this.module) {
            const items = this.studioService.createSidebarExplorerPath(this.module.Id, "Module");
            this.$rootScope.explorerExpandedItems.push(...items);
            this.$rootScope.explorerExpandedItems.push(this.module.ModuleType.toLowerCase() + '-modules');

            this.$rootScope.explorerCurrentItem = this.module.Id;
        }
    }

    setForm() {
        this.form = this.validationService.init({
            ActionType: {
                required: true,
            },
            ActionName: {
                id: "txtActionName" +
                    (this.action.Id ? this.action.Id : ""),
                rule: (value) => {
                    if (this.step > 1 && !value) return false;

                    return true;
                },
                required: true,
            },
            Event: {
                id: "drpEvent" + (this.action.Id ? this.action.Id : ""),
                rule: (value) => {
                    if (this.step > 1 && !value) return false;

                    return true;
                },
                required: true,
            },
            ParentId: {
                id: "drpParentId" + (this.action.Id ? this.action.Id : ""),
                rule: (value) => {
                    if (this.step > 1 && this.action.Event == "OnActionCompleted" && !value)
                        return false;

                    return true;
                },
                required: true,
            },
            ParentActionTriggerCondition: {
                id: "drpParentActionTriggerCondition" + (this.action.Id ? this.action.Id : ""),
                rule: (value) => {
                    if (this.step > 1 && this.action.Event == "OnActionCompleted" && (value == undefined || value == null))
                        return false;

                    return true;
                },
                required: true,
            }
        },
            true,
            this.$scope,
            "$.action"
        );
    }

    gotoStep(step) {
        if (this.step < step && this.step < 4) {
            // goto next step
            this.form.validated = true;
            this.form.validator(this.action);

            if (this.form.valid) {
                this.step = step;

                this.form.validated = false;

                this.stepsValid = this.step;
            }
        } else if (this.step > step && this.step > 1) {
            // goto prev step
            this.step = step;
        }

        if (this.step == step) {
            if (typeof this.stepsCallback[step] == "function") {
                const $this = this;
                this.stepsCallback[step].apply(this).then(() => {
                });
            }

            var newUrl = this.globalService.replaceUrlParam("st", step);
            this.globalService.pushState(newUrl);
        }
    }

    onStepClick(step) {
        if (!this.running && this.stepsValid >= step) this.gotoStep(step);
    }

    onPrevStepClick() {
        this.gotoStep(this.step - 1);
    }

    onNextStepClick() {
        this.gotoStep(this.step + 1);
    }

    onSelectActionTypeClick(actionType) {
        this.action.ActionType = actionType.ActionType;
        this.action.ActionType = actionType.ActionType;
        this.action.HasResult = actionType.HasResult;
        this.action.ResultType = actionType.ResultType;

        this.actionType = actionType;

        if (this.stepsValid == 1) {
            this.gotoStep(2);
            this.stepsValid = 2;
        }

        delete this.isActionLoaded;
    }

    initActionBuilder() {
        const defer = this.$q.defer();

        const actionComponent = `<${this.actionType.ActionComponent} controller="$" action="$.action" services="$.services" actions="$.actions" module-actions="$.moduleActions" variables="$.variables" data-view-models="$.viewModels" data-fields="$.fields" ${this.actionType.ComponentSubParams}></${this.actionType.ActionComponent}>`;

        this.$timeout(() => {
            $("#pnlActionBuilder" + (this.action.Id ? this.action.Id : "")).html(this.$compile(actionComponent)(this.$scope));
        });

        this.isActionLoaded = true;

        defer.resolve();

        return defer.promise;
    }

    onSaveActionClick() {
        this.form.validated = true;
        this.form.validator(this.action);
        if (this.form.valid) {
            this.$deferredBroadcast(this.$scope, "onValidateAction").then(
                (IsNotValid) => {
                    if (!IsNotValid) {
                        this.running = "save-action";
                        this.awaitAction = {
                            title: "Saving Action",
                            subtitle: "Just a moment for saving the action...",
                        };

                        this.currentTabKey = this.$rootScope.currentTab.key;

                        this.apiService.post("Module", "SaveAction", this.action).then((data) => {
                            this.action.Id = data;

                            this.$deferredBroadcast(this.$scope, "onSaveAction", { isNewAction: this.isNewAction, }).then(() => {
                                this.notifyService.success(
                                    "Action updated has been successfully"
                                );

                                this.$scope.$emit("onUpdateCurrentTab", {
                                    id: this.action.Id,
                                    title: this.action.ActionName,
                                    key: this.currentTabKey,
                                });

                                delete this.awaitAction;
                                delete this.running;
                            }, (error) => {
                                if (this.isNewAction) delete this.action.Id;

                                this.awaitAction.isError = true;
                                this.awaitAction.subtitle = error.statusText;
                                this.awaitAction.desc =
                                    this.globalService.getErrorHtmlFormat(error);

                                this.notifyService.error(error.data.Message);

                                delete this.running;
                            }
                            );
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
                }
            );
        }
    }

    onCancelActionClick() {
        $("#wnCreateAction").modal("hide");

        delete this.action;

        this.$scope.actionForm.$submitted = false;

        location.reload();
    }

    onDeleteActionClick() {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this imaginary action!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((willDelete) => {
            if (willDelete) {
                this.running = "get-actions";
                this.awaitAction = {
                    title: "Remove Action",
                    subtitle: "Just a moment for removing action...",
                };

                this.apiService.post("Module", "DeleteAction", { Id: this.action.Id }).then(
                    (data) => {
                        this.notifyService.success("Action deleted has been successfully");

                        this.onCloseWindow();

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