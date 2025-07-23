import variableEditWidget from "./variable-edit.html";

export class CreateModuleVariablesController {
    constructor(
        $scope,
        $rootScope,
        $timeout,
        $q,
        globalService,
        apiService,
        validationService,
        notificationService
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.$q = $q;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;

        this.variableEditWidget = variableEditWidget;

        this.$rootScope.createModuleValidatedStep.push(4);

        $scope.$on("onCreateModuleValidateStep4", (e, task, args) => {
            this.validateStep.apply(this, [task, args]);
        });

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName('id');

        this.running = "get-module-variables";
        this.awaitAction = {
            title: "Get Variables",
            subtitle: "Just a moment for get the module variables...",
        };

        this.apiService.get("Module", "GetModuleVariables", { moduleId: id }).then((data) => {
            this.variableTypes = data.VariableTypes;
            this.variables = data.Variables ?? [];
            this.viewModels = data.ViewModels;

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });

        this.setForm();
    }

    setForm() {
        this.form = this.validationService.init({
            VariableType: {
                id: "drpVariableType",
                required: true,
            },
            VariableName: {
                id: "txtVariableName",
                required: true,
            },
            Scope: {
                id: "drpVariableScope",
                required: true,
            }
        },
            true,
            this.$scope,
            "$.variable"
        );
    }

    onAddVariableClick() {
        let moduleId = this.globalService.getParameterByName('id');
        this.variable = { ModuleId: moduleId };

        this.workingMode = "variable-edit";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onEditVariableClick(variable) {
        this.variable = _.clone(variable);

        this.workingMode = "variable-edit";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onSaveVariableClick() {
        this.form.validated = true;
        this.form.validator(this.variable);
        if (this.form.valid) {
            this.running = "save-variable";
            this.awaitAction = {
                title: "Saving Variable",
                subtitle: "Just a moment for saving the current variable...",
            };

            this.apiService.post("Module", "SaveModuleVariable", this.variable).then((data) => {
                if (!this.variable.Id) {
                    this.variable = data;
                    this.variables.push(this.variable);
                }
                else {
                    _.filter(this.variables, (v) => { return v.Id == this.variable.Id }).map((v) => {
                        this.variables[this.variables.indexOf(v)] = data;
                    });
                }

                this.notifyService.success("The variable updated has been successfully");

                this.disposeWorkingMode();

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                delete this.running;
            });
        }
    }

    onCancelVariableClick() {
        this.disposeWorkingMode();
    }

    onDeleteVariableClick(variableId, $index) {
        this.globalService.deleteConfirmAlert().then((yes) => {
            if (yes) {
                this.running = "delete-variable";
                this.awaitAction = {
                    title: "Removing Variable",
                    subtitle: "Just a moment for removing the current variable...",
                };

                this.apiService.post("Module", "DeleteModuleVariable", { Id: variableId }).then((data) => {
                    this.notifyService.success("The variable removed has been successfully");

                    this.variables.splice($index, 1);

                    delete this.awaitAction;
                    delete this.running;
                }, (error) => {
                    this.awaitAction.isError = true;
                    this.awaitAction.subtitle = error.statusText;
                    this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                    delete this.running;
                });
            }
        });
    }

    onPrevStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 3 });
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 5 });
    }

    validateStep(task, args) {
        task.wait(() => {
            const $defer = this.$q.defer();

            $defer.resolve(true);

            return $defer.promise;
        });
    }

    disposeWorkingMode() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }
}