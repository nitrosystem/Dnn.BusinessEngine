import Swal from 'sweetalert2'

export class CreateScenarioController {
    constructor(
        $rootScope,
        $compile,
        $scope,
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

        this.onPageLoad();
    }

    //#region Scenario Global Methods

    onPageLoad() {
        this.scenario = _.clone(this.$rootScope.scenario);

        this.setForm();
    }

    setForm() {
        this.form = this.validationService.init({
            ScenarioName: {
                id: "txtScenarioName",
                required: true,
            },
            ScenarioTitle: {
                id: "txtScenarioTitle",
                required: true,
            },
            DatabaseObjectPrefix: {
                id: "txtDatabaseObjectPrefix",
                required: true,
            },
        },
            true,
            this.$scope,
            "$.scenario"
        );
    }

    onSaveScenarioClick() {
        this.form.validated = true;
        this.form.validator(this.scenario);
        if (this.form.valid) {
            this.running = "save-scenario";
            this.awaitAction = {
                title: "Saving Scenario",
                subtitle: "Just a moment for saving the scenario...",
            };

            this.apiService.post("Studio", "SaveScenario", this.scenario).then((data) => {
                const isNewScenario = !this.scenario.ScenarioId;

                this.scenario = data;
                this.$rootScope.scenario = _.clone(this.scenario);

                this.notifyService.success("Scenario updated has been successfully");

                return;
                
                if (isNewScenario) {
                    const url = this.globalService.replaceUrlParam("s", this.scenario.ScenarioName);

                    this.globalService.pushState(url);

                    this.$timeout(() => location.reload());
                }

                delete this.awaitAction;
                delete this.running;
            },
                (error) => {
                    this.awaitAction.isError = true;
                    this.awaitAction.subtitle = error.statusText;
                    this.awaitAction.desc =
                        this.globalService.getErrorHtmlScenarioat(error);

                    this.notifyService.error(error.data.Message);

                    delete this.running;
                }
            );
        }
    }

    onCancelScenarioClick() { }

    onRenderScenarioModulesClick() {
        this.running = "render-all-modules";
        this.awaitAction = {
            title: "Rendering All Modules",
            subtitle: "Just a moment for rendering the scenario all modules...",
        };
        this.moduleBuilderService.rebuildScenarioModules(this.scenario.ScenarioId, this.$scope);
    }

    onDeleteScenarioClick() {
        let timerInterval;
        Swal.fire({
            title: "<h4>Are you sure remove scenario and child items?</h4><b></b>",
            html: '<p>Be aware that this operation is highly risky and irreversible. Upon confirmation, the entire scenario and all its dependencies will be permanently deleted.</p>',
            icon: "warning",
            timer: 20000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes, Remove it!",
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
            this.running = "remove-scenario";
            this.awaitAction = {
                title: "Remove Scenario",
                subtitle: "Just a moment for removing the scenario and child items...",
            };

            this.apiService.post("Studio", "DeleteScenarioAndChilds", { ID: this.scenario.ScenarioId }).then((data) => {
                this.notifyService.success("The removed scenario and child items have been successfully deleted.");

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc =
                    this.globalService.getErrorHtmlScenarioat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        });
    }

    //#endregion 

    //#region Export Scenario

    onShowExportScenarioWinClick() {
        this.export = { PackageName: this.scenario.ScenarioName, PackageVersion: '01.00.00' };

        this.workingMode = "export-scenario";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onExportScenarioClick() {
        //this.form.validated = true;
        //this.form.validator(this.scenario);
        if (1 == 1 || this.form.valid) {
            this.running = "export-scenario";
            this.awaitAction = {
                title: "Export Scenario",
                subtitle: "Just a moment for exporting the scenario components...",
            };

            this.apiService.post("Studio", "ExportScenario", this.export).then((data) => {
                this.notifyService.success("export scenario has been successfully");

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc =
                    this.globalService.getErrorHtmlScenarioat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }
    }

    onCancelExportScenarioClick() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }

    //#endregion

    //#region Import Scenario

    onShowImportScenarioWinClick() {
        this.workingMode = "import-scenario";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onImportScenarioFileUploadChange($file, $invalidFiles) {
        if ($file) this.import = { files: $file, PackageType: 2 }
    }

    onImportScenarioClick() {
        if (this.import.files) {
            this.running = "export-scenario";
            this.awaitAction = {
                title: "Export Scenario",
                subtitle: "Just a moment for exporting the scenario components...",
            };

            this.apiService.uploadFile("Studio", "ImportFile", this.import).then((data) => {
                this.notifyService.success("import scenario has been successfully");

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc =
                    this.globalService.getErrorHtmlScenarioat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }
    }

    onCancelImportScenarioClick() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }

    //#endregion
}