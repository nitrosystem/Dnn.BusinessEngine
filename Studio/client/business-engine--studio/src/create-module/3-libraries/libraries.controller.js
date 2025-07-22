import Swal from 'sweetalert2'

import libraryEditWidget from "./library-edit.html";
import resourceEditWidget from "./resource-edit.html";

export class CreateModuleLibrariesController {
    constructor(
        $scope,
        $rootScope,
        $timeout,
        $q,
        globalService,
        apiService,
        validationService,
        studioService,
        notificationService) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.$q = $q;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.studioService = studioService;
        this.notifyService = notificationService;
        this.sortableOptions = {
            stop: (e, ui) => {
                const target = $(e.target).data('type');
                const isSorted = target == 0
                    ? (this.moduleCustomLibraries.length > 1 ? true : false)
                    : (this.moduleCustomResources.length > 1 ? true : false);
                if (isSorted) {
                    var items = target == 0 ? this.moduleCustomLibraries : this.moduleCustomResources

                    let list = [];
                    for (let index = 0; index < items.length; index++) {
                        items[index].LoadOrder = index;
                        const item = { Id: items[index].Id, LoadOrder: items[index].LoadOrder };
                        list.push(item);
                    }

                    this.onSortItems(target, list);
                }
            }
        };

        this.libraryEditWidget = libraryEditWidget
        this.resourceEditWidget = resourceEditWidget

        this.$rootScope.createModuleValidatedStep.push(3);

        $scope.$on("onCreateModuleValidateStep3", (e, task, args) => {
            this.validateStep.apply(this, [task, args]);
        });

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName('id');

        this.running = "get-module-libraries";
        this.awaitAction = {
            title: "Get Libraries",
            subtitle: "Just a moment for get the module libraries...",
        };

        this.apiService.get("Module", "GetModuleCustomLibraries", { moduleId: id }).then((data) => {
            this.libraries = data.Libraries;
            this.moduleCustomLibraries = data.ModuleCustomLibraries ?? [];
            this.moduleCustomResources = data.ModuleCustomResources ?? [];

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
        this.library = { ...this.library, ...(this.libraries.find(l => l.Id == this.library.LibraryId) ?? {}) }
        const { Id, ...rest } = this.library;

        this.running = "load-library-resources";
        this.awaitAction = {
            title: "Loading Library Resources",
            subtitle: "Just a moment for loading the library resources...",
        };

        this.apiService.get("Module", "GetLibraryResources", { libraryId: this.library.LibraryId }).then((data) => {
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
            this.moduleCustomLibraries.push(this.library);

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
            this.moduleCustomResources.push(this.resource);

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

    onSortItems(target, items) {
        this.running = "sort-items";
        this.awaitAction = {
            title: "Sort Items",
            subtitle: "Just a moment for sorting items...",
        };

        this.apiService.post("Module", "SortModuleCustomLibraries", items, { target: target }).then((data) => {
            this.notifyService.success("Items sorted has been successfully");

            delete this.awaitAction;
            delete this.running;
        }, (error) => {
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

                this.apiService.post("Module", "DeleteModuleCustomLibrary", { Id: id }).then((data) => {
                    this.notifyService.success("Library deleted has been successfully");

                    this.moduleCustomLibraries.splice($index, 1);

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

                this.apiService.post("Module", "DeleteModuleCustomResource", { Id: id }).then((data) => {
                    this.notifyService.success("Resource deleted has been successfully");

                    this.moduleCustomResources.splice($index, 1);

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

    onPreviousStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 2 });
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 4 });
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