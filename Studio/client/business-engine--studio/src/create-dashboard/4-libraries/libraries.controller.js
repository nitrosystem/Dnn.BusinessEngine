import Swal from 'sweetalert2'

import libraryEditWidget from "./library-edit.html";
import resourceEditWidget from "./resource-edit.html";

export class CreateDashboardLibrariesController {
    constructor(
        $scope,
        $rootScope,
        $timeout,
        $q,
        globalService,
        apiService,
        studioService,
        notificationService) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.$q = $q;
        this.globalService = globalService;
        this.apiService = apiService;
        this.studioService = studioService;
        this.notifyService = notificationService;
        this.sortableOptions = {
            stop: (e, ui) => {
                let target;
                let targetType = $(e.target).data('type');
                if (targetType == 'library')
                    target = this.customLibraries;
                else
                    target = customResources;

                let list = [];
                for (let index = 0; index < target.length; index++) {
                    target[index].LoadOrder = index;
                    const item = { ModuleId: this.moduleId, Id: target[index].Id, LoadOrder: target[index].LoadOrder };
                    list.push(item);
                }

                this.onSortItems(targetType, list);
            }
        };

        this.libraryEditWidget = libraryEditWidget
        this.resourceEditWidget = resourceEditWidget

        this.$rootScope.createDashboardValidatedStep.push(4);

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        $scope.$on("onCreateDashboardValidateStep4", (e, task, args) => {
            task.wait(() => {
                const $defer = this.$q.defer();

                $defer.resolve(true);

                return $defer.promise;
            });
        });

        this.onPageLoad();
    }

    onPageLoad() {
        this.moduleId = this.globalService.getParameterByName('id');

        this.running = "get-module-libraries";
        this.awaitAction = {
            title: "Get Libraries",
            subtitle: "Just a moment for get the module libraries...",
        };

        this.apiService.get("Module", "GetDashboardLibraries", { moduleId: this.moduleId, }).then((data) => {
            this.libraries = data.Libraries;
            this.customLibraries = data.DashboardLibraries ?? [];
            this.customResources = data.DashboardResources ?? [];

            this.library = {};

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

    onPreviousStepClick() {
        this.$scope.$emit('onCreateDashboardChangeStep', { step: 3 });
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateDashboardChangeStep', { step: 5 });
    }

    validateStep(task, args) {
        task.wait(() => {
            const $defer = this.$q.defer();

            $defer.resolve(true);

            return $defer.promise;
        });
    }

    onAddLibraryClick() {
        let moduleId = this.globalService.getParameterByName('id');
        this.library = { ModuleId: moduleId };

        this.workingMode = "library-edit";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onAddResourceClick() {
        let moduleId = this.globalService.getParameterByName('id');
        this.resource = { ModuleId: moduleId };

        this.workingMode = "resource-edit";
        this.$scope.$emit("onShowRightWidget", { controller: this });
    }

    onLibraryChange() {
        this.library = { ...this.library, ...(this.libraries.find(l => l.Id == this.library.Id) ?? {}) }
        const { Id, ...rest } = this.library;

        this.running = "load-library-resources";
        this.awaitAction = {
            title: "Loading Library Resources",
            subtitle: "Just a moment for loading the library resources...",
        };

        this.apiService.get("Module", "GetLibraryResources", { libraryId: this.library.Id }).then((data) => {
            this.library = this.globalService.deepClone(rest);
            this.library.Resources = data;

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

    onSaveLibraryClick() {
        this.running = "save-library";
        this.awaitAction = {
            title: "Save Library",
            subtitle: "Just a moment for saving library...",
        };

        this.apiService.post("Module", "SaveModuleCustomLibrary", this.library).then((data) => {
            this.notifyService.success("Library updated has been successfully");

            this.library.Id = data;
            this.customLibraries.push(this.library);

            this.disposeWorkingMode();

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

    onCancelLibraryClick() {
        this.disposeWorkingMode();
    }

    onSaveResourceClick() {
        this.running = "save-resource";
        this.awaitAction = {
            title: "Save Resource",
            subtitle: "Just a moment for saving resource...",
        };

        this.apiService.post("Module", "SaveModuleCustomResource", this.resource).then((data) => {
            this.notifyService.success("Library updated has been successfully");

            this.resource.Id = data;
            this.customResources.push(this.resource);

            this.disposeWorkingMode();

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

    onCancelResourceClick() {
        this.disposeWorkingMode();
    }

    onSortItems(targetType, items) {
        this.running = "sort-items";
        this.awaitAction = {
            title: "Sort Items",
            subtitle: "Just a moment for sorting items...",
        };

        this.apiService.post("Module", "SortModuleCustomResources", items, { target: targetType == 'library' ? 0 : 1 }).then((data) => {
            this.notifyService.success("Items sorted has been successfully");

            delete this.awaitAction;
            delete this.running;
        },
            (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            }
        );
    }

    onDeleteLibraryClick(id, $index) {
        let timerInterval;
        Swal.fire({
            title: "<h4>Are you sure remove this item</h4><b></b>",
            icon: "warning",
            timer: 5000,
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
            if (result.isConfirmed) {
                this.running = "remove-library";
                this.awaitAction = {
                    title: "Remove Library",
                    subtitle: "Just a moment for removing library from module libraries...",
                };

                this.apiService.post("Module", "DeleteCustomLibrary", { Id: id }).then((data) => {
                    this.notifyService.success("Library deleted has been successfully");

                    this.customLibraries.splice($index, 1);

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

    onDeleteResourceClick(id, $index) {
        let timerInterval;
        Swal.fire({
            title: "<h4>Are you sure remove this item</h4><b></b>",
            icon: "warning",
            timer: 5000,
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
            if (result.isConfirmed) {
                this.running = "remove-resource";
                this.awaitAction = {
                    title: "Remove Resource",
                    subtitle: "Just a moment for removing resource from module resources...",
                };

                this.apiService.post("Module", "DeleteCustomResource", { Id: id }).then((data) => {
                    this.notifyService.success("Resource deleted has been successfully");

                    this.customResources.splice($index, 1);

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

    onCancelEditClick() {
        this.disposeWorkingMode();
    }

    disposeWorkingMode() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }
}