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
                rule: (value) => {
                    if (/^[A-Z][A-Za-z0-9]*$/.test(value) == false)
                        return "Scenario name is not valid";
                    else
                        return true;
                },
                required: true,
            },
            ScenarioTitle: {
                id: "txtScenarioTitle",
                required: true,
            },
            DatabaseObjectPrefix: {
                id: "txtDatabaseObjectPrefix",
                rule: (value) => {
                    if (/^[a-z]{1,9}_$/.test(value) == false)
                        return "Database object prefix is not valid";
                    else
                        return true;
                },
                required: true,
            },
        },
            true,
            this.$scope,
            "$.scenario"
        );
    }

    onSetValidNameClick() {
        this.scenario.ScenarioName = this.globalService.normalizeName(this.scenario.ScenarioName);
    }

    onSetValidDbObjectPrefixClick() {
        this.scenario.DatabaseObjectPrefix = this.sanitizeDatabasePrefix(this.scenario.DatabaseObjectPrefix);
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
                const isNewScenario = !this.scenario.Id;

                this.scenario = data;
                this.$rootScope.scenario = _.clone(this.scenario);

                this.notifyService.success("Scenario updated has been successfully");

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
                        this.globalService.getErrorHtmlFormat(error);

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
        this.moduleBuilderService.rebuildScenarioModules(this.scenario.Id, this.$scope);
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

            this.apiService.post("Studio", "DeleteScenario", { Id: this.scenario.Id }).then((data) => {
                if (data) this.notifyService.success("The removed scenario have been successfully deleted.");

                location.reload();

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                delete this.running;

                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);
            });
        });
    }

    //#endregion 

    //#region Export Scenario

    onShowExportScenarioWinClick() {
        this.workingMode = "export-scenario";
        this.$scope.$emit("onShowRightWidget", { controller: this });

        this.exportData = {
            GenerateEntityScripts: true
        };
    }

    onExportScenarioClick() {
        this.exportRequest = {
            ExportName: this.scenario.ScenarioName,
            ExportScope: 0,
            Channel: this.scenario.ScenarioName,
            Version: '01.00.00',
            Params: {
                ScenarioId: this.scenario.Id,
                GenerateEntityScripts: this.exportData.GenerateEntityScripts
            }
        }

        this.running = "export-scenario";
        this.awaitAction = {
            title: "Export Scenario",
            subtitle: "Just a moment for exporting the scenario components...",
        };

        this.apiService.post("Studio", "Export", this.exportRequest).then((data) => {
            this.notifyService.success("export scenario has been successfully");

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
        if ($file) {
            this.running = "load-import-file";
            this.awaitAction = {
                title: "Load Import File",
                subtitle: "Just a moment for loading the file...",
            };

            this.apiService.uploadFile("Studio", "LoadExportedFile", { files: $file }).then((data) => {
                const exportedData = JSON.parse(data.ExportedJson);
                // const scenarioJson = exportedData.Items.find(i => i.ComponentName === 'Scenario');
                // const scenario = JSON.parse(scenarioJson.ExportedJson);
                const modulesJson = exportedData.Items.find(i => i.ComponentName === 'Module');
                const modules = JSON.parse(modulesJson.ExportedJson) ?? [];

                this.exportedFile = data.ExportedFile;
                this.pages = data.Pages;
                this.exportedData = {
                    // scenario: scenario,
                    modules: modules.filter(m => m.SiteModuleId) ?? []
                }

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

    onImportScenarioClick() {
        if (this.exportedData) {
            this.importRequest = {
                ImportScope: 0,
                Channel: this.scenario.ScenarioName,
                ExportedFile: this.exportedFile,
                Params: {
                    ModulesNewPages: {}
                }
            }

            for (const module of this.exportedData.modules ?? []) {
                if (module.NewPageId)
                    this.importRequest.Params.ModulesNewPages[module.Id] = module.NewPageId;
            }


            this.running = "import-scenario";
            this.awaitAction = {
                title: "Import Scenario",
                subtitle: "Just a moment for importing the scenario components...",
            };

            this.apiService.post("Studio", "Import", this.importRequest).then((data) => {
                this.notifyService.success("import scenario has been successfully");

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

    onCancelImportScenarioClick() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }

    //#endregion

    sanitizeDatabasePrefix(input) {
        if (!input) return '';

        // حذف فاصله‌ها و کاراکترهای غیر مجاز
        let clean = input
            .toLowerCase()
            .replace(/[^a-z]/g, ''); // فقط حروف کوچک انگلیسی مجازند

        // حداکثر 9 کاراکتر قبل از _
        clean = clean.slice(0, 9);

        // اطمینان از اینکه با _ تمام شود
        if (!clean.endsWith('_')) {
            clean += '_';
        }

        return clean;
    }

}