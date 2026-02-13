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
        actionCenterService,
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
        this.actionCenterService = actionCenterService;
        this.notifyService = notificationService;
        this.$deferredBroadcast = $deferredBroadcast;

        this.$scope.$parent.createModuleValidatedStep.push(1);

        $scope.$on("onCreateModuleValidateStep1", (e, task, args) => {
            task.wait(() => {
                return this.validateStep();
            });
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

    validateModule(compareFields) {
        this.form.validated = true;
        this.form.validator(this.module);
        return this.form.valid;
    }

    onSaveModuleClick() {
        if (this.validateModule()) {
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

                this.module.Id = data;
                this.oldModule = angular.copy(this.module);

                this.$scope.$emit("onUpdateCurrentTab", {
                    id: this.module.Id,
                    title: this.module.ModuleName,
                    key: this.currentTabKey,
                });

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }
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

    validateStep() {
        if (this.validateModule()) {
            if (this.module.ModuleTitle !== this.oldModule.ModuleTitle) {
                // this.actionCenterService.addTask({
                //     taskId: `${this.module.Id}-SaveBasicOptions`,
                //     icon: 'codicon codicon-save-as',
                //     title: 'Save Module Basic Options...',
                //     subtitle: 'The module changes require saving.',
                //     percent: 0,
                //     actions: [
                //         {
                //             text: 'Apply',
                //             buttonCssClass: 'btn-primary',
                //             callback: () => this.onSaveModuleClick(),
                //             timer: 15
                //         },
                //         {
                //             text: 'Cancel',
                //             callback: () => this.actionCenterService.removeTask(`${this.module.Id}-SaveBasicOptions`),
                //         },
                //     ]
                // });
            }

            return true;
        }
    }

    //#region Export Module

    onShowExportModuleWinClick() {
        this.exportRequest = {
            ExportName: this.module.ModuleName,
            ExportScope: 1,
            Channel: this.$rootScope.scenario.ScenarioName,
            Version: '01.00.00',
            Params: {
                ModuleId: this.module.Id
            }
        }

        this.workingMode = "export-module";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onExportModuleClick() {
        //this.form.validated = true;
        //this.form.validator(this.module);
        if (1 == 1 || this.form.valid) {
            this.running = "export-module";
            this.awaitAction = {
                title: "Export Module",
                subtitle: "Just a moment for exporting the module components...",
            };

            this.apiService.post("Studio", "Export", this.exportRequest).then((data) => {
                this.notifyService.success("export module has been successfully");

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                delete this.running;

                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc =
                    this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);
            });
        }
    }

    onCancelExportModuleClick() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }

    //#endregion

    //#region Import Module

    onShowImportModuleWinClick() {
        this.importParam = {
            ScenarioId: this.module.ScenarioId,
            ParentId: this.module.ParentId
        }

        if (this.module.Id) {
            this.importParam.ModuleId = this.module.Id;
            this.importParam.SiteModuleId = this.module.SiteModuleId;
            this.importParam.ModuleName = this.module.ModuleName;
            this.importParam.ModuleTitle = this.module.ModuleTitle;

            // let timerInterval;
            // Swal.fire({
            //     title: ' For import module into current module, you must delete the module.',
            //     html: '<h5>WARNING!</h5><p>Are you Sure!?</p><b></b>',
            //     icon: "warning",
            //     timer: 5000,
            //     timerProgressBar: true,
            //     showCancelButton: true,
            //     confirmButtonColor: "#d33",
            //     cancelButtonColor: "#3085d6",
            //     confirmButtonText: "Yes, delete it!",
            //     backdrop: false,
            //     didOpen: () => {
            //         const timer = Swal.getPopup().querySelector("b");
            //         timerInterval = setInterval(() => {
            //             timer.textContent = `${Swal.getTimerLeft()}`;
            //         }, 100);
            //     },
            //     willClose: () => {
            //         clearInterval(timerInterval);
            //     }
            // }).then((result) => {
            //     if (result.isConfirmed) {
            //         this.running = "delete-module";
            //         this.awaitAction = {
            //             title: "Delete Module",
            //             subtitle: "Just a moment for deleting App Model...",
            //         };

            //         this.apiService.post("Module", "DeleteModule", { Id: this.module.Id }).then((data) => {
            //             this.notifyService.success("Module deleted has been successfully");

            //             this.workingMode = "import-module";
            //             this.$scope.$emit("onShowRightWidget", { controller: this });

            //             this.module = null;

            //             delete this.awaitAction;
            //             delete this.running;
            //         }, (error) => {
            //             this.awaitAction.isError = true;
            //             this.awaitAction.subtitle = error.statusText;
            //             this.awaitAction.desc =
            //                 this.globalService.getErrorHtmlFormat(error);

            //             this.notifyService.error(error.data.Message);

            //             delete this.running;
            //         });
            //     }
            // });
        }
        // else  {
        this.workingMode = "import-module";
        this.$scope.$emit("onShowRightWidget", { controller: this });
        // }
    }

    onImportModuleFileUploadChange($file, $invalidFiles) {
        if ($file) {
            this.importFile = $file;
        }
    }

    onImportModuleClick() {
        if (this.importFile) {
            this.running = "export-module";
            this.awaitAction = {
                title: "Export Module",
                subtitle: "Just a moment for exporting the module components...",
            };

            this.import = {
                ImportScope: 1,
                Channel: this.$rootScope.scenario.ScenarioName,
                Params: {
                    ScenarioId: this.importParam.ScenarioId,
                    ModuleId: this.importParam.ModuleId,
                    ParentId: this.importParam.ParentId,
                    SiteModuleId: this.importParam.SiteModuleId ?? this.globalService.getParameterByName("d"),
                    ModuleName: this.importParam.ModuleName,
                    ModuleTitle: this.importParam.ModuleTitle,
                    DeleteModuleBeforeImport: this.importParam.DeleteModuleBeforeImport
                }
            }

            this.apiService.uploadFile("Studio", "Import", { files: this.importFile }, { ImportOptions: JSON.stringify(this.import) }).then((data) => {
                this.notifyService.success("import module has been successfully");

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                delete this.running;

                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);
            });
        }
    }

    onCancelImportModuleClick() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }

    //#endregion

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}