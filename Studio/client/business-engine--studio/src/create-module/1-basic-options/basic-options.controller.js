import { GlobalSettings } from "../../angular/angular-configs/global.settings";
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

        this.running = "get-module";
        this.awaitAction = {
            title: "Loading Module",
            subtitle: "Just a moment for loading module...",
        };

        this.apiService.get("Module", "GetModuleBasicOptions", { ...(id && { moduleId: id }) }).then((data) => {
            this.module = data || { ScenarioId: GlobalSettings.scenarioId };
            this.oldModule = angular.copy(this.module);

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
                rule: (value) => {
                    if (/^[A-Z][A-Za-z0-9]*$/.test(value) == false)
                        return "Module name is not valid";
                    else
                        return true;
                },
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

    onSetModuleValidNameClick() {
        this.module.ModuleName = this.globalService.normalizeName(this.module.ModuleName);
    }

    onSaveModuleClick() {
        const $defer = this.$q.defer();

        this.form.validated = true;
        this.form.validator(this.module);

        var changes = this.globalService.compareTwoObject(this.module, this.oldModule);
        if (Object.keys(changes).length === 0)
            $defer.resolve(true);
        else if (this.form.valid) {
            const id = this.globalService.getParameterByName("id");
            this.module.Id = id;

            if (!this.module.SiteModuleId) {
                const siteModuleId = this.globalService.getParameterByName("d");
                this.module.SiteModuleId = siteModuleId;
            }

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
        }

        return $defer.promise;
    }

    onSidebarTabClick(tab) {
        this.currentSidebarTab = tab;
    }

    onCancelModuleClick() {
        this.onCloseWindow();
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

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}