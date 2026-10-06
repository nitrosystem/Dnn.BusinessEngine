import { GlobalHelper, UrlHelper } from "../../../shared/helpers";
import { GlobalUtil } from "../../../shared/utils";

export class CreateDashboardBasicOptionsController {
    constructor(
        $rootScope,
        $scope,
        apiService,
        validationService,
        notificationService,
        baseOptions
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.baseOptions = baseOptions;

        $scope.$parent.createDashboardValidatedStep.push(1);
        $scope.$on('onCreateDashboardValidateStep1', (e, defer) => {
            this._validateStep(defer);
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');

        this.running = 'get-dashboard';
        this.awaitAction = {
            title: 'Loading Basic Options',
            subtitle: 'Just a moment for loading basic options of the dashboard...',
        };

        this.apiService.get('Module', 'GetDashboardBasicOptions', { moduleId: id }).then((data) => {
            this.dashboard = data.Dashboard ?? {
                ScenarioId: this.baseOptions.scenarioId
            };

            this._setForm();

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

    onSetModuleValidNameClick() {
        this.dashboard.ModuleName = GlobalHelper.normalizeName(this.dashboard.ModuleName);
    }

    onSaveDashboardClick() {
        this.form.validated = true;
        this.form.validator(this.dashboard);
        if (this.form.valid) {
            this.dashboard.SiteModuleId = UrlHelper.getParameterByName('d') || this.dashboard.SiteModuleId;

            this.running = 'save-dashboard';
            this.awaitAction = {
                title: 'Saving Dashboard',
                subtitle: 'Just a moment for saving the dashboard...',
            };

            this.currentTabKey = this.$rootScope.currentTab?.key;

            this.apiService.post('Module', 'SaveDashboardBasicInfo', this.dashboard).then((data) => {
                this.notifyService.success('Dashboard updated has been successfully');

                this.dashboard.Id = data.DashboardId;
                this.dashboard.ModuleId = data.ModuleId;

                this.$scope.$emit('onUpdateCurrentTab', {
                    id: this.dashboard.ModuleId,
                    title: this.dashboard.ModuleName,
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

    onCancelDashboardClick() {
        this.onCloseWindow();
    }

    onNextStepClick() {
        this.$scope.$emit('onCreateDashboardChangeStep', { step: 2 });
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
            ModuleName: {
                id: 'txtModuleName',
                rule: (value) => {
                    if (/^[A-Za-z][A-Za-z0-9_]*$/.test(value) == false)
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
            '$.dashboard'
        );
    }

    _validateStep(defer) {
        if (this.dashboard.Id)
            defer.resolve(true);
        else {
            this.form.validated = true;
            this.form.validator(this.dashboard);

            const isValid = this.form.valid;
            if (isValid) this.notifyService.info('You must be save the Dashboard.');

            defer.resolve(false);
        }
    }

    //#endregion
}