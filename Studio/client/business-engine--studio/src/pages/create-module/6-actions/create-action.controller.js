import Swal from "sweetalert2";
import { GlobalHelper, UrlHelper, JsonHelper } from "../../../shared/helpers";
import { GlobalUtil } from "../../../shared/utils";

export class CreateModuleCreateActionController {
    constructor(
        $rootScope,
        $scope,
        $compile,
        $timeout,
        $q,
        apiService,
        validationService,
        notificationService,
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$compile = $compile;
        this.$timeout = $timeout;
        this.$q = $q;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;

        this.stepsCallback = { 3: this._initActionBuilder };
        this.actionBuilder = {};
        this.events = [];

        $rootScope.$on('onUpdateVariables', (e, args) => {
            if (args.moduleId === this.module.Id)
                this._getVariables();
        });

        $scope.$on('onSyncActionParamsWithServiceParams', (e, args) => {
            const service = args.service || {};
            const action = args.action || {};

            action.Params = action.Params || [];
            service.Params.forEach(sp => {
                if (!action.Params.some(ap => ap.ParamName === sp.ParamName))
                    action.Params.push({ ParamName: sp.ParamName });
            });
        });

        const actionConditionsDslWatch = $scope.$watch('$.action.ActionConditionsDsl', (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.actionConditionsDslModified = true

                actionConditionsDslWatch();
            }
        });

        const beforeExecuteActionDslWatch = $scope.$watch('$.action.BeforeExecuteActionDsl', (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.beforeExecuteActionDslModified = true

                beforeExecuteActionDslWatch();
            }
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const moduleId = UrlHelper.getParameterByName('module');
        const fieldId = UrlHelper.getParameterByName('field');
        const fieldType = UrlHelper.getParameterByName('type');
        const id = UrlHelper.getParameterByName('id');
        const step = parseInt(UrlHelper.getParameterByName('st'), 10);

        this.running = 'get-action';
        this.awaitAction = {
            title: 'Loading Action',
            subtitle: 'Just a moment for loading action...',
        };

        this.apiService.get('Module', 'GetAction', {
            moduleId: moduleId,
            fieldId: fieldId,
            actionId: id,
            fieldType: fieldType,
        }).then((data) => {
            this.module = { Id: moduleId };
            this.events = data.Events;
            this.actionTypes = data.ActionTypes;
            this.builtinFunctions = data.DslBuiltInFunctions;
            this.actions = data.Actions;
            this.action = data.Action;
            this._variables = data.Variables;

            if (!this.action) {
                this.action = {
                    ModuleId: moduleId,
                    FieldId: fieldId,
                    Settings: {},
                };
                this.step = 1;
            } else {
                this.actionType = this.actionTypes.find(at => at.ActionType === this.action.ActionType);

                this.step = step || 2;
                if (this.step === 3 && this.actionType.HideModuleBuilder)
                    this.step = 4;

                this.$scope.$emit('onUpdateCurrentTab', {
                    id: this.action.Id,
                    title: this.action.ActionName,
                });
            }

            this._setForm();
            this._gotoStep(this.step);
            this._setSuggestions();

            this.running = null;
            this.awaitAction = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    onSetValidNameClick() {
        this.action.ActionName = GlobalHelper.normalizeName(this.action.ActionName);
    }

    onSelectActionTypeClick(actionType) {
        this.actionType = actionType;
        this.action.ActionType = actionType.ActionType;
        this.action.HasResult = actionType.HasResult;

        this._gotoStep(2);

        this._setSuggestions();
    }

    onEventChange() {
        if (this.action.Event !== 'OnActionCompleted')
            this.action.ParentId = null;
    }

    onGotoActionsPageClick() {
        const page = {
            page: 'create-action',
            id: this.existsAction.ActionId,
            subParams: {
                module: this.existsAction.ModuleId,
                type: this.existsAction.FieldType,
                field: this.existsAction.FieldId
            }
        };
        this.$scope.$emit('onGotoPage', page);
    }

    onSaveActionClick() {
        if (this._validateCurrentStep()) {
            this.running = 'save-action';
            this.awaitAction = {
                title: 'Saving Action',
                subtitle: 'Just a moment for saving the action...',
            };

            this.apiService.post('Module', 'SaveAction', this.action).then((data) => {
                if (data.ActionId) {
                    if (!this.action.Id)
                        this.notifyService.success('Action has been created successfully.');
                    else
                        this.notifyService.info('Action has been updated successfully.');

                    this.action.Id = data.ActionId;
                    this.action.ServiceType = data.ServiceType;

                    this.$scope.$emit('onUpdateCurrentTab', {
                        id: this.action.Id,
                        title: this.action.ActionName,
                        key: this.$rootScope.currentTab?.key,
                    });

                    this.$scope.$emit('onUpdateActions', {
                        moduleId: this.action.ModuleId,
                        fieldId: this.action.FieldId
                    });
                }

                //ExecuteOrder server salidated
                this.duplicateExecuteOrderValidation = data.ValidationResults.DuplicateExecuteOrderValidation || '';
                if (this.duplicateExecuteOrderValidation) {
                    this.existsAction = data.ExistsAction;
                }

                //ActionConditionsDsl server salidated
                this.actionConditionsDslValidation = data.ValidationResults.ActionConditionsDslValidation || '';
                if (this.actionConditionsDslValidation)
                    this.notifyService.warning(`Action conditions dsl is not valid :: ${this.actionConditionsDslValidation}`);
                if (data.ValidationResults.ConditionIsTrueNotFound) {
                    this.actionConditionsDslValidation += '\n' +
                        `_ConditionIsTrue not found in action condition dsl`;
                    this.notifyService.warning(`_ConditionIsTrue not found in action condition dsl`);
                }

                //BeforeExecuteActionDsl server salidated
                this.beforeExecuteActionDslValidation = data.ValidationResults.BeforeExecuteActionDslValidation;
                if (this.beforeExecuteActionDslValidation)
                    this.notifyService.warning(`Before action dsl is not valid :: ${this.beforeExecuteActionDslValidation}`);

                //ActionResultsDsl server salidated
                this.actionResultsDslValidation = data.ValidationResults.ActionResultsDslValidation;
                if (this.actionResultsDslValidation)
                    this.notifyService.warning(`Action results dsl is not valid :: ${this.actionResultsDslValidation}`);

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

    onDeleteActionClick() {
        let timerInterval;
        Swal.fire({
            title: 'Are you sure?',
            html: '<p>Once deleted, you will not be able to recover this imaginary entity!</p><b></b>',
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
                this.running = 'delete-action';
                this.awaitAction = {
                    title: 'Delete Action',
                    subtitle: 'Just a moment for deleting action...',
                };

                this.apiService.post('Module', 'DeleteAction', { Id: this.action.Id }).then((data) => {
                    if (data) {
                        this.notifyService.info('Action has been deleted successfully.');
                        this.onCloseWindow();
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

    onStepClick(step) {
        if (!this.running) this._gotoStep(step);
    }

    onPreviousStepClick() {
        this._gotoStep(this.step - 1);
    }

    onNextStepClick() {
        this._gotoStep(this.step + 1);
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            ActionType: {
                required: true,
            },
            ActionName: {
                id: 'txtActionName' + (this.action.Id ?? ''),
                rule: (value) => {
                    if (this.step > 1) {
                        if (!value)
                            return false;
                        else if (/^[A-Za-z][A-Za-z0-9_]*$/.test(value) === false)
                            return 'Action name is not valid';
                    }

                    return true;
                },
                required: true,
            },
            Event: {
                id: 'drpEvent' + (this.action.Id ?? ''),
                rule: (value) => {
                    if (!value && this.step > 1) return false;

                    return true;
                },
                required: true,
            },
            ParentId: {
                id: 'drpParentId' + (this.action.Id ?? ''),
                rule: (value) => {
                    if (!value && this.step > 1 && this.action.Event === 'OnActionCompleted')
                        return false;

                    return true;
                },
                required: true,
            },
            ParentActionTriggerCondition: {
                id: 'drpParentActionTriggerCondition' + (this.action.Id ?? ''),
                rule: (value) => {
                    if (!value && this.step > 1 && this.action.Event === 'OnActionCompleted')
                        return false;

                    return true;
                },
                required: true,
            },
            ExecuteOrder: {
                id: 'txtExecuteOrder' + (this.action.Id ?? ''),
                rule: (value) => {
                    //Be careful not use ===
                    if (value == undefined && this.step > 1)
                        return false;

                    return true;
                },
                required: true,
            },
        },
            true,
            this.$scope,
            '$.action'
        );
    }

    _gotoStep(step) {
        if (this.step < step) {
            if (this.step <= 2 && !this._validateCurrentStep(true)) return;
            if (step >= 3 && !this.action.Id) {
                this.notifyService.warning('You must save the Action first.');
                return;
            }

            this.step = step;
            if (this.step === 3 && this.actionType.HideModuleBuilder) this.step = 4;

        } else if (this.step > step) {
            this.step = step;
        }

        if (this.step !== step) return;

        this.stepsCallback[step]?.call(this);

        const newUrl = UrlHelper.replaceUrlParam('st', step);
        UrlHelper.pushState(newUrl);
    }

    _validateCurrentStep(resetValidate) {
        this.form.validated = true;
        this.form.validator(this.action);
        if (!this.form.valid) return false;

        if (resetValidate) this.form.validated = false;
        return true;
    }

    _initActionBuilder() {
        const defer = this.$q.defer();

        const actionComponent =
            `<${this.actionType.ActionComponent} 
                controller="$"
                action="$.action"
                services="$.services"
                service-type="${this.action.ServiceType || this.actionType.ActionType}"
                search-text="${this.action.ServiceName ?? ''}"
                suggestions="$.suggestions"
                ${this.actionType.ComponentSubParams ?? ''}>
            </${this.actionType.ActionComponent}>`;

        this.$timeout(() => {
            $('#pnlActionBuilder' + (this.action.Id ?? '')).html(
                this.$compile(actionComponent)(this.$scope)
            );
        });

        defer.resolve();

        return defer.promise;
    }

    _getVariables() {
        this.apiService.get('Module', 'GetModuleVariablesForModuleBuilder', { moduleId: this.module.Id }).then((data) => {
            this._variables = data;
            this._setSuggestions();
        });
    }

    _setSuggestions() {
        this.suggestions = {
            _PageParam: {},
            _CurrentUserId: 0,
        };

        for (const variable of this._variables) {
            const baseObject = variable.Properties?.reduce((acc, prop) => {
                acc[prop.PropertyName] = {};
                return acc;
            }, {});

            if (variable.VariableType === 'AppModelList') {
                this.suggestions[variable.VariableName] = [baseObject];
            } else {
                this.suggestions[variable.VariableName] = baseObject;
            }
        }

        if (this.actionType?.DslSuggestions && JsonHelper.isJsonString(this.actionType.DslSuggestions))
            this.suggestions = { ...this.suggestions, ...JSON.parse(this.actionType.DslSuggestions) }
    }

    //#endregion
}