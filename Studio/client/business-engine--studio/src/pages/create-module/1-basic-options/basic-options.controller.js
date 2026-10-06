import Swal from 'sweetalert2'
import { GlobalUtil } from '../../../shared/utils';
import { GlobalHelper, UrlHelper } from '../../../shared/helpers';

export class CreateModuleBasicOptionsController {
    constructor(
        $rootScope,
        $scope,
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
        this.baseOptions = baseOptions;

        $scope.$parent.createModuleValidatedStep.push(1);
        $scope.$on('onCreateModuleValidateStep1', (e, defer) => {
            this._validateStep(defer);
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');

        this.running = 'get-module';
        this.awaitAction = {
            title: 'Loading Module',
            subtitle: 'Just a moment for loading module...',
        };

        this.apiService.get('Module', 'GetModuleBasicOptions', { moduleId: id }).then((data) => {
            this.module = data.Module ?? {
                ScenarioId: this.baseOptions.scenarioId
            };
            this.fieldTypeExists = data.FieldTypeExists;

            this._setForm();

            this.running = null;
            this.awaitAction = null;
        });
    }

    onSetModuleValidNameClick() {
        this.module.ModuleName = GlobalHelper.normalizeName(this.module.ModuleName);
    }

    onGotoExtensionClick() {
        this.$scope.$emit('onGotoPage', {
            page: 'extensions',
            activityBar: 'extensions'
        });
    }

    onSaveModuleClick() {
        if (!this._checkExistsFieldType()) return;

        this.form.validated = true;
        this.form.validator(this.module);
        if (this.form.valid) {
            this.module.SiteModuleId = UrlHelper.getParameterByName('d') || this.module.SiteModuleId;

            this.running = 'save-module';
            this.awaitAction = {
                title: 'Saving Module',
                subtitle: 'Just a moment for saving the module...',
            };

            this.currentTabKey = this.$rootScope.currentTab?.key;

            this.apiService.post('Module', 'SaveModuleBasicOptions', this.module).then((data) => {
                if (!this.module.Id)
                    this.notifyService.success('Module has been created successfully.');
                else
                    this.notifyService.info('Module has been updated successfully.');

                this.module.Id = data;

                this.$scope.$emit('onUpdateCurrentTab', {
                    id: this.module.Id,
                    title: this.module.ModuleName,
                    key: this.currentTabKey,
                });

                this.$rootScope.refreshSidebarExplorerItems();

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

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            ScenarioId: {
                id: 'drpScenarioId',
                required: true,
            },
            ModuleType: {
                id: 'drpModuleType',
                required: true,
            },
            ModuleName: {
                id: 'txtModuleName',
                rule: (value) => {
                    if (/^[A-Za-z][A-Za-z0-9_]*$/.test(value) === false)
                        return 'Module name is not valid';
                    else
                        return true;
                },
                required: true,
            },
            ModuleTitle: {
                id: 'txtModuleTitle',
                required: true,
            }
        },
            true,
            this.$scope,
            '$.module'
        );
    }

    _validateStep(defer) {
        if (this.module.Id) {
            defer.resolve(true);
        }
        else if (this._checkExistsFieldType()) {
            this.form.validated = true;
            this.form.validator(this.module);
            if (this.form.valid) this.notifyService.info('You must save the Module first.');
            defer.resolve(false);
        }
    }

    _checkExistsFieldType() {
        if (!this.fieldTypeExists) {
            this.activeShine = true;
            this.$timeout(() => this.activeShine = null, 3000);
            return false;
        }

        return true;
    }

    //#endregion
}