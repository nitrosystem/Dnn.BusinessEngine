import Swal from 'sweetalert2';
import { GlobalUtil } from '../../../shared/utils';
import { GlobalHelper, UrlHelper } from '../../../shared/helpers';
import variableEditWidget from './variable-edit.html';

export class CreateModuleVariablesController {
    constructor(
        $scope,
        $rootScope,
        $timeout,
        apiService,
        validationService,
        notificationService,
        baseOptions
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.variableEditWidget = variableEditWidget;
        this.baseOptions = baseOptions;

        this.$scope.$parent.createModuleValidatedStep.push(4);
        $scope.$on('onCreateModuleValidateStep4', (e, defer) => {
            defer.resolve(true);
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');
        const scenarioId = this.baseOptions.scenarioId;

        this.running = 'get-module-variables';
        this.awaitAction = {
            title: 'Get Variables',
            subtitle: 'Just a moment for get the module variables...',
        };

        this.apiService.get('Module', 'GetModuleVariables', { scenarioId, moduleId: id }).then((data) => {
            this.variableTypes = data.VariableTypes;
            this.appModels = data.AppModels;
            this.variables = data.Variables ?? [];
            this.module = { Id: id };
            this.filter = {};

            this._purgeData();
            this._setForm();
            this.$timeout(() => {
                this.$scope.$broadcast('onEditVariable');
            });

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

    onSetValidNameClick() {
        this.variable.VariableName = GlobalHelper.normalizeName(this.variable.VariableName);
    }

    onSwapVariableClick(variable, state) {
        const source = !variable.GroupName
            ? this.nonGroupItems
            : this.groupItems[variable.GroupName];

        const list = source.sort((a, b) => (a.ViewOrder ?? 0) - (b.ViewOrder ?? 0));
        const index = list.findIndex(v => v.Id === variable.Id);
        const newIndex = state === 1 ? index - 1 : index + 1;
        if (newIndex < 0 || newIndex >= source.length) return;

        [list[index], list[newIndex]] = [list[newIndex], list[index]];
        list.forEach((v, i) => { v.ViewOrder = i; });

        const sortedIds = [...source]
            .sort((a, b) => (a.ViewOrder ?? 0) - (b.ViewOrder ?? 0))
            .map(v => v.Id);

        this.running = 'sort-variables';
        this.awaitAction = {
            title: 'Sorting Variables',
            subtitle: 'Just a moment for sorting variables...',
        };

        this.apiService.post('Module', 'SortModuleVariables', { SortedIds: sortedIds })
            .then(() => {
                this.notifyService.info('Variables has been sorted successfully.');

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

    onAddVariableClick() {
        this.variable = { ModuleId: this.module.Id };
        this.workingMode = 'variable-edit';
        this.$scope.$emit('onShowRightWidget');
    }

    onEditVariableClick(variable) {
        this.variable = GlobalHelper.clone(variable);
        this.workingMode = 'variable-edit';
        this.$scope.$emit('onShowRightWidget');
    }

    onSaveVariableClick() {
        this.form.validated = true;
        this.form.validator(this.variable);
        if (this.form.valid) {
            this.running = 'save-variable';
            this.awaitAction = {
                title: 'Saving Variable',
                subtitle: 'Just a moment for saving the current variable...',
            };

            this.apiService.post('Module', 'SaveModuleVariable', this.variable).then((data) => {
                if (!this.variable.Id) {
                    this.notifyService.success('Variable has been created successfully.');

                    this.variable.Id = data;
                    this.variables.push(this.variable);
                }
                else {
                    this.notifyService.info('Variable has been updated successfully.');

                    const index = this.variables.findIndex(v => v.Id === this.variable.Id);
                    if (index >= 0) this.variables[index] = this.variable;
                }

                this._purgeData();
                this.onCloseEditVariableWindow();
                this.form.validated = false;
                this.$scope.$emit('onUpdateVariables', { moduleId: this.variable.ModuleId });

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

    onDeleteVariableClick(variable) {
        let timerInterval;
        Swal.fire({
            title: 'Are you sure?',
            html: '<p>Once deleted, you will not be able to recover this imaginary variable!</p><b></b>',
            icon: 'warning',
            timer: 5000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!',
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
                this.running = 'delete-variable';
                this.awaitAction = {
                    title: 'Removing Variable',
                    subtitle: 'Just a moment for removing the current variable...',
                };

                this.apiService.post('Module', 'DeleteModuleVariable', { Id: variable.Id }).then((data) => {
                    if (data) {
                        this.notifyService.info('Variable has been deleted successfully.');

                        const index = this.variables.findIndex(v => v.Id === variable.Id);
                        this.variables.splice(index, 1);
                        if (this.variables.some(v => !!v.GroupName)) this._setGroupItems();

                        this.$scope.$emit('onUpdateVariables', { moduleId: variable.ModuleId });
                    }

                    this.awaitAction = null;
                    this.running = null;
                }, (error) => {
                    this.awaitAction.isError = true;
                    this.awaitAction.subtitle = error.statusText;
                    this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                    this.running = null;
                });
            }
        });
    }

    onPreviousStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 3 });
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateModuleChangeStep', { step: 5 });
    }

    onCloseEditVariableWindow() {
        this.$scope.$emit('onHideRightWidget');
        this.$timeout(() => this.workingMode = null, 200);
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            VariableType: {
                id: 'drpVariableType',
                required: true,
            },
            VariableName: {
                id: 'txtVariableName',
                rule: (value) => {
                    if (this.step > 1 && !value)
                        return false;

                    if (value && /^[A-Za-z][A-Za-z0-9_]*$/.test(value) === false)
                        return 'Variable name is not valid';

                    return true;
                },
                required: true,
            },
            Scope: {
                required: true,
            },
            AppModelId: {
                id: 'drpAppModel',
                rule: (value) => {
                    if (!value && (this.variable.VariableType === 'AppModel' ||
                        this.variable.VariableType === 'AppModelList')
                    )
                        return false;

                    return true;
                },
            },
        },
            true,
            this.$scope,
            '$.variable'
        );
    }

    _purgeData() {
        this._setModelName();
        if (this.variables.some(v => v.GroupName))
            this._setGroupItems();
    }

    _setModelName() {
        this.variables.forEach(variable => {
            if (variable.AppModelId)
                variable.ModelName = this.appModels.find(m => m.Id === variable.AppModelId)?.ModelName;
        });
    }

    _setGroupItems() {
        const nonGroupItems = [];
        const groupItems = {};

        this.variables.forEach(item => {
            if (!item.GroupName) {
                nonGroupItems.push(item);
            } else {
                if (!groupItems[item.GroupName])
                    groupItems[item.GroupName] = [];

                groupItems[item.GroupName].push(item);
            }
        });

        this.nonGroupItems = nonGroupItems;
        this.groupItems = groupItems;
    }
}