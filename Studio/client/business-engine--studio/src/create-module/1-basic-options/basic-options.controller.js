import { GlobalSettings } from "../../angular-configs/global.settings";
import Swal from 'sweetalert2'
import 'animate.css';

export class CreateModuleBasicOptionsController {
    constructor(
        $scope,
        $rootScope,
        studioService,
        $timeout,
        $q,
        globalService,
        apiService,
        validationService,
        notificationService,
        $deferredBroadcast
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.studioService = studioService;
        this.$timeout = $timeout;
        this.$q = $q;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.$deferredBroadcast = $deferredBroadcast;

        this.$rootScope.createModuleValidatedStep.push(1);

        $scope.$on("onCreateModuleValidateStep1", (e, task, args) => {
            this.validateStep.apply(this, [task, args]);
        });

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");
        var step = this.globalService.getParameterByName("st");
        this.step = step && id ? parseInt(step) : 1;
        this.stepsValid = this.step;

        this.running = "get-module";
        this.awaitAction = {
            title: "Loading Module",
            subtitle: "Just a moment for loading module...",
        };

        this.apiService.get("Module", "GetModuleBasicOptions", { moduleId: isNaN(id) ? id : null }).then((data) => {
            this.module = data;
            this.oldModule = angular.copy(this.module);

            if (this.module) {
                this.$scope.$emit('onFillBasicModuleOptions', { module: this.module });
            }
            else if (!this.module) {
                this.module = {
                    ScenarioId: GlobalSettings.scenarioId,
                };
            }

            delete this.running;
            delete this.awaitAction;
        });

        this.setForm();
    }

    setForm() {
        this.form = this.validationService.init({
            ScenarioId: {
                id: "drpScenarioId",
                required: true,
            },
            ModuleType: {
                id: "drpModuleType",
                required: true,
            },
            ModuleName: {
                id: "txtModuleName",
                required: true,
            },
            ModuleTitle: {
                id: "txtModuleTitle",
                required: true,
            }
        },
            true,
            this.$scope,
            "$.module"
        );
    }

    onPreviousStepClick() {
        Swal.fire({
            title: '<p style="font-size:1.3rem">There is no step before the first step!! Of course, if you want, you can enter the matrix world!!!.</p>',
            html: '<i class="codicon codicon-broadcast" style="font-size:5rem;"></i>',
            confirmButtonText: '<i class="codicon codicon-smiley mt-1 b-icon-2"></i>',
            showClass: {
                popup: `
                  animate__animated
                  animate__fadeInUp
                  animate__faster
                `
            },
            hideClass: {
                popup: `
                  animate__animated
                  animate__fadeOutDown
                  animate__faster
                `
            }
        });
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 2 });
    }

    validateStep(task, args) {
        task.wait(() => {
            return this.onSaveModuleClick();
        });
    }

    validateModuleName() {
        var $defer = this.$q.defer();

        if (_.isEmpty(this.module.ModuleName)) {
            this.moduleNameIsValid = false;
            this.moduleNameIsEmpty = true;
            this.moduleNameIsValidPattern = true;

            $defer.reject();
        } else if (/^[a-z]{1}[a-z|0-9|-]{1,30}$/gim.test(this.module.ModuleName) == false) {
            this.moduleNameIsValid = false;
            this.moduleNameIsValidPattern = false;

            $defer.reject();
        } else {
            this.running = "check-module-name";
            this.awaitAction = {
                title: "Checking Module Name",
                subtitle: "Just a moment for checking name of the module...",
            };

            this.moduleNameIsValidPattern = true;

            this.apiService.post("Module", "CheckModuleName", {
                ScenarioId: this.module.ScenarioId,
                ModuleId: this.module.Id,
                ModuleName: this.module.ModuleName
            }).then((data) => {
                this.moduleNameIsValid = data;

                if (data) $defer.resolve();
                else $defer.reject();

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                $defer.reject();

                delete this.running;
            });
        }

        return $defer.promise;
    }

    onSaveModuleClick() {
        const $defer = this.$q.defer();

        this.form.validated = true;
        this.form.validator(this.module);

        if (this.module == this.oldModule)
            $defer.resolve(true);
        else {
            if (this.form.valid) {
                this.validateModuleName().then(() => {
                    const id = this.globalService.getParameterByName("id");
                    if (!isNaN(id)) this.module.DnnModuleId = id;

                    this.running = "save-module";
                    this.awaitAction = {
                        title: "Saving Module",
                        subtitle: "Just a moment for saving the module...",
                    };

                    this.currentTabKey = this.$rootScope.currentTab.key;

                    this.apiService.post("Module", "SaveModuleBasicOptions", this.module).then((data) => {
                        this.notifyService.success("Module updated has been successfully");

                        this.$rootScope.refreshSidebarExplorerItems();

                        let isNew = !this.module.Id;

                        this.module.Id = data;
                        this.oldModule = angular.copy(this.module);

                        if (isNew) this.$scope.$emit('onFillBasicModuleOptions', { module: this.module });

                        this.$scope.$emit("onUpdateCurrentTab", {
                            id: this.module.Id,
                            title: this.module.ModuleName,
                            key: this.currentTabKey,
                        });

                        $defer.resolve(true);

                        delete this.awaitAction;
                        delete this.running;
                    }, (error) => {
                        this.awaitAction.isError = true;
                        this.awaitAction.subtitle = error.statusText;
                        this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                        this.notifyService.error(error.data.Message);

                        $defer.reject();

                        delete this.running;
                    });
                });
            }
            else
                $defer.resolve(false);
        }

        return $defer.promise;
    }

    onSidebarTabClick(tab) {
        this.currentSidebarTab = tab;
    }

    onCancelModuleClick() {
        this.onCloseWindow();
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}