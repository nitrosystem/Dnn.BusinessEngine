import Swal from 'sweetalert2'
import { GlobalUtil } from '../../../shared/utils';
import { UrlHelper } from '../../../shared/helpers';
import libraryEditWidget from './library-edit.html';
import resourceEditWidget from './resource-edit.html';

export class CreateModuleLibrariesController {
    constructor(
        $rootScope,
        $scope,
        $timeout,
        apiService,
        validationService,
        notificationService,
        baseOptions) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.baseOptions = baseOptions;

        this.libraryEditWidget = libraryEditWidget
        this.resourceEditWidget = resourceEditWidget

        $scope.$parent.createModuleValidatedStep.push(3);
        $scope.$on('onCreateModuleValidateStep3', (e, defer) => {
            defer.resolve(true);
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');

        this.running = 'get-module-libraries';
        this.awaitAction = {
            title: 'Get Custom Libraries/Resources',
            subtitle: 'Just a moment for get the module custom libraries or resources...',
        };

        this.apiService.get('Module', 'GetModuleCustomLibraries', { moduleId: id }).then((data) => {
            this.libraries = data.Libraries;
            this.moduleCustomLibraries = data.ModuleCustomLibraries ?? [];
            this.moduleCustomResources = data.ModuleCustomResources ?? [];
            this.module = { Id: id };

            this._setForm()

            this.awaitAction = null;
            this.running = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    onSwapItemClick(itemId, target, state) {
        const source = target === 0
            ? this.moduleCustomLibraries
            : this.moduleCustomResources;

        const list = source.sort((a, b) => (a.LoadOrder ?? 0) - (b.LoadOrder ?? 0));
        const index = list.findIndex(v => v.Id === itemId);
        const newIndex = state === 1 ? index - 1 : index + 1;
        if (newIndex < 0 || newIndex >= source.length) return;

        [list[index], list[newIndex]] = [list[newIndex], list[index]];
        list.forEach((v, i) => { v.LoadOrder = i; });

        const sortedIds = [...source]
            .sort((a, b) => (a.LoadOrder ?? 0) - (b.LoadOrder ?? 0))
            .map(v => v.Id);

        this.running = 'sort-items';
        this.awaitAction = {
            title: 'Sort Items',
            subtitle: 'Just a moment for sorting items...',
        };

        this.apiService.post('Module', 'SortModuleCustomLibraries', { SortedIds: sortedIds }, { target: target })
            .then(() => {
                this.notifyService.info('Items has been sorted successfully.');

                this.awaitAction = null;
                this.running = null;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                this.notifyService.error(error.data);

                this.running = null;
            });
    }

    onPreviousStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 2 });
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 4 });
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#region Library Methods

    onAddLibraryClick() {
        this.library = { ModuleId: this.module.Id };
        this.workingMode = 'library-edit';
        this.$scope.$emit('onShowRightWidget');
    }

    onLibraryChange() {
        const library = this.libraries.find(l => l.Id === this.libraryId);
        this.library = {
            ...this.library,
            ...{
                ModuleId: this.module.Id,
                LibraryName: library.LibraryName,
                Version: library.Version,
                Logo: library.Logo,
                Summary: library.Summary
            }
        }
        this.libraryResources = library.Resources;
    }

    onSaveLibraryClick() {
        const scenarioId = this.baseOptions.scenarioId;

        this.libraryForm.validated = true;
        this.libraryForm.validator(this.library);
        if (this.libraryForm.valid) {
            this.running = 'save-library';
            this.awaitAction = {
                title: 'Save Library',
                subtitle: 'Just a moment for saving library...',
            };

            this.apiService.post('Module', 'SaveModuleCustomLibrary', this.library, { scenarioId }).then((data) => {
                this.notifyService.success('Library has been added successfully to the list of required libraries for the module.');

                this.library.Id = data;
                this.moduleCustomLibraries.push(this.library);

                this._disposeWorkingMode();

                this.onPageLoad();

                this.awaitAction = null;
                this.running = null;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                this.notifyService.error(error.data);

                this.running = null;
            });
        }
    }

    onDeleteLibraryClick(id, parentId, index) {
        const scenarioId = this.baseOptions.scenarioId;
        let timerInterval;
        Swal.fire({
            title: '<h4>Are you sure remove this item</h4><b></b>',
            icon: 'warning',
            timer: 5000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, Remove it!',
            backdrop: false,
            didOpen: () => {
                const timer = Swal.getPopup().querySelector('b');
                timerInterval = setInterval(() => {
                    timer.textContent = `${Swal.getTimerLeft()}`;
                }, 100);
            },
            willClose: () => {
                clearInterval(timerInterval);
            }
        }).then((result) => {
            if (result.isConfirmed) {
                this.running = 'remove-library';
                this.awaitAction = {
                    title: 'Remove Library',
                    subtitle: 'Just a moment for removing library from module libraries...',
                };

                this.apiService.post('Module', 'DeleteModuleCustomLibrary',
                    { Id: id, ParentId: parentId }, { scenarioId }).then((data) => {
                        if (data) {
                            this.notifyService.info('Library has been deleted successfully.');
                            this.moduleCustomLibraries.splice(index, 1);
                        }

                        this.awaitAction = null;
                        this.running = null;
                    }, (error) => {
                        this.awaitAction.isError = true;
                        this.awaitAction.subtitle = error.statusText;
                        this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                        this.notifyService.error(error.data);

                        this.running = null;
                    });
            }
        });
    }

    onCloseEditLibraryWindow() {
        this._disposeWorkingMode();
    }

    //#endregion

    //#region Resource Methods

    onAddResourceClick() {
        this.resource = { ModuleId: this.module.Id };
        this.workingMode = 'resource-edit';
        this.$scope.$emit('onShowRightWidget');
    }

    onSaveResourceClick() {
        const scenarioId = this.baseOptions.scenarioId;

        this.resourceForm.validated = true;
        this.resourceForm.validator(this.resource);
        if (this.resourceForm.valid) {
            this.running = 'save-resource';
            this.awaitAction = {
                title: 'Save Resource',
                subtitle: 'Just a moment for saving resource...',
            };

            this.apiService.post('Module', 'SaveModuleCustomResource', this.resource, { scenarioId }).then((data) => {
                this.notifyService.success('Resource has been added successfully to the list of required resources for the module.');

                this.resource.Id = data;
                this.moduleCustomResources.push(this.resource);

                this._disposeWorkingMode();

                this.awaitAction = null;
                this.running = null;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                this.notifyService.error(error.data);

                this.running = null;
            });
        }
    }

    onDeleteResourceClick(id, parentId, index) {
        const scenarioId = this.baseOptions.scenarioId;
        let timerInterval;
        Swal.fire({
            title: '<h4>Are you sure remove this item</h4><b></b>',
            icon: 'warning',
            timer: 5000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, Remove it!',
            backdrop: false,
            didOpen: () => {
                const timer = Swal.getPopup().querySelector('b');
                timerInterval = setInterval(() => {
                    timer.textContent = `${Swal.getTimerLeft()}`;
                }, 100);
            },
            willClose: () => {
                clearInterval(timerInterval);
            }
        }).then((result) => {
            if (result.isConfirmed) {
                this.running = 'remove-resource';
                this.awaitAction = {
                    title: 'Remove Resource',
                    subtitle: 'Just a moment for removing resource from module resources...',
                };

                this.apiService.post('Module', 'DeleteModuleCustomResource',
                    { Id: id, ParentId: parentId }, { scenarioId }).then((data) => {
                        if (data) {
                            this.notifyService.info('Resource has been deleted successfully.');
                            this.moduleCustomResources.splice(index, 1);
                        }

                        this.awaitAction = null;
                        this.running = null;
                    }, (error) => {
                        this.awaitAction.isError = true;
                        this.awaitAction.subtitle = error.statusText;
                        this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                        this.notifyService.error(error.data);

                        this.running = null;
                    });
            }
        });
    }

    onCloseEditResourceWindow() {
        this._disposeWorkingMode();
    }

    //#endregion

    //#endregion

    //#region Private Methods

    _setForm() {
        this.libraryForm = this.validationService.init({
            LibraryName: {
                id: 'drpLibraryName',
                required: true,
            },
            Version: {
                required: true,
            },
            LoadOrder: {
                id: 'txtLoadOrder',
                required: true,
            }
        },
            true,
            this.$scope,
            '$.library'
        );

        this.resourceForm = this.validationService.init({
            AddressType: {
                id: 'drpAddressType',
                required: true,
            },
            ResourceContentType: {
                id: 'drpResourceContentType',
                required: true,
            },
            ResourcePath: {
                id: 'txtResourcePath',
                required: true,
            },
            LoadOrder: {
                id: 'txtLoadOrder',
                required: true,
            }
        },
            true,
            this.$scope,
            '$.resource'
        );
    }

    _disposeWorkingMode() {
        this.$scope.$emit('onHideRightWidget');
        this.$timeout(() => this.workingMode = null, 200);
    }

    //#endregion
}