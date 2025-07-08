import { GlobalSettings } from "../../angular-configs/global.settings";
import Swal from 'sweetalert2'
import 'animate.css';

export class CreateDashboardBasicOptionsController {
    constructor(
        $scope,
        $rootScope,
        $compile,
        $timeout,
        $q,
        globalService,
        apiService,
        validationService,
        notificationService,
        $deferredBroadcast
    ) {
        "ngInject";

        this.$rootScope = $rootScope;
        this.$compile = $compile;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.$q = $q;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.$deferredBroadcast = $deferredBroadcast;

        this.$rootScope.createDashboardValidatedStep.push(1);

        $scope.$on("onCreateDashboardValidateStep1", (e, task, args) => {
            task.wait(() => {
                const $defer = this.$q.defer();

                this.onSaveDashboardClick().then((isValid) => {
                    $defer.resolve(isValid);
                });

                return $defer.promise;
            });
        });

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");
        const parent = this.globalService.getParameterByName("parent");
        const moduleId = parent || id;

        if (moduleId) {
            this.running = "get-dashboard";
            this.awaitAction = {
                title: "Loading Basic Options",
                subtitle: "Just a moment for loading basic options of the dashboard...",
            };

            this.apiService.get("Module", "GetDashboardBasicOptions", { moduleId: moduleId || null }).then((data) => {
                this.dashboard = data.Dashboard;
                this.oldDashboard = _.clone(this.dashboard)

                delete this.running;
                delete this.awaitAction;

                this.$timeout(() => {
                    this.hideStepPreloader = true;
                }, 100);
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }
        else {
            this.dashboard = {
                ScenarioId: GlobalSettings.scenarioId,
            };
        }

        this.setForm();
    }

    setForm() {
        this.form = this.validationService.init({
            ScenarioId: {
                id: "drpScenarioId",
                required: true,
            },
            ModuleName: {
                id: "txtModuleName",
                required: true,
            },
            ModuleTitle: {
                id: "txtModuleTitle",
                required: true,
            },
            DashboardType: {
                id: "drpDashboardType",
                required: true,
            },
            UniqueName: {
                id: "txtUniqueName",
                rule: (value) => {
                    if (this.dashboard.DashboardType == 2) return true;
                    else if (this.dashboard.DashboardType == 1)
                        return this.dashboard.UniqueName ? true : false;
                },
                required: true,
            },
        },
            true,
            this.$scope,
            "$.dashboard"
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
        this.$scope.$emit('onCreateDashboardChangeStep', { step: 2 });
    }

    validateModuleName() {
        var $defer = this.$q.defer();

        if (this.dashboard.Id) {
            $defer.resolve(true);
            return $defer.promise;
        }

        if (_.isEmpty(this.dashboard.ModuleName)) {
            this.moduleNameIsValid = false;
            this.moduleNameIsEmpty = true;
            this.moduleNameIsValidPattern = true;

            $defer.resolve(false);
        } else if (/^[a-z]{1}[a-z|0-9|-]{1,30}$/gim.test(this.dashboard.ModuleName) == false) {
            this.moduleNameIsValid = false;
            this.moduleNameIsValidPattern = false;

            $defer.resolve(false);
        } else {
            this.running = "check-module-name";
            this.awaitAction = {
                title: "Checking Module Name",
                subtitle: "Just a moment for checking name of the module...",
            };

            this.moduleNameIsValidPattern = true;

            this.apiService.post("Module", "CheckModuleName", {
                ScenarioId: this.dashboard.ScenarioId,
                ModuleId: this.dashboard.ModuleId,
                ModuleName: this.dashboard.ModuleName
            }).then((data) => {
                this.moduleNameIsExists = data;
                this.moduleNameIsValid = data;

                $defer.resolve(data);

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                $defer.resolve(false);;

                delete this.running;
            });
        }

        return $defer.promise;
    }

    onSaveDashboardClick() {
        const $defer = this.$q.defer();

        if (this.globalService.deepEqual(this.dashboard, this.oldDashboard)) {
            this.notifyService.info('The basic properties of the dashboard remain unchanged!.');
            $defer.resolve(true);
        }
        else {
            this.form.validated = true;
            this.form.validator(this.dashboard);
            if (this.form.valid) {
                this.validateModuleName().then((isValid) => {
                    if (!isValid) {
                        $defer.resolve(isValid);
                        return $defer.promise;
                    }

                    this.dashboard.DnnModuleId = this.globalService.getParameterByName("d") || this.dashboard.DnnModuleId;

                    this.running = "save-dashboard";
                    this.awaitAction = {
                        title: "Saving Dashboard",
                        subtitle: "Just a moment for saving the dashboard...",
                    };

                    this.currentTabKey = this.$rootScope.currentTab.key;

                    this.apiService.post("Module", "SaveDashboardBasicInfo", this.dashboard).then((data) => {
                        this.notifyService.success("Dashboard updated has been successfully");

                        this.dashboard.Id = data.DashboardId;
                        this.dashboard.ModuleId = data.ModuleId;
                        this.oldDashboard = this.globalService.deepClone(this.dashboard);

                        this.$rootScope.refreshSidebarExplorerItems();

                        this.$scope.$emit("onUpdateCurrentTab", {
                            id: this.dashboard.ModuleId,
                            title: this.dashboard.ModuleName,
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

                        delete this.running;
                    });
                });
            }
            else
                $defer.resolve(false);
        }

        return $defer.promise;
    }

    onCancelDashboardClick() {
        this.onCloseWindow();
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}