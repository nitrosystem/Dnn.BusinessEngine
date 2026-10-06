import { GlobalUtil } from '../../../shared/utils';
import { GlobalHelper, UrlHelper } from '../../../shared/helpers';

export class SelectScenarioController {
    constructor(
        $rootScope,
        $scope,
        apiService,
        validationService,
        notificationService
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        this.running = 'get-scenarios';
        this.awaitAction = {
            title: 'Loading Scenarios',
            subtitle: 'Just a moment for loading scenarios...',
        };

        this.apiService.get('Studio', 'GetScenarios').then((data) => {
            this.scenarios = data;
            this.scenario = {};
            this._setForm();

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

    onSelectScenarioClick() {
        this.form.validated = true;
        this.form.validator(this.scenario);
        if (this.form.valid) {
            const scenario = this.$rootScope.scenarios.find(s => s.Id === this.scenario.Id);
            if (scenario) {
                const url = UrlHelper.replaceUrlParam('s', scenario.ScenarioName);
                location.href = url;
            }
        }
    }

    onAddScenarioClick() {
        window['wnSelectScenario'].hide();

        const page = UrlHelper.getParameterByName('m');
        const sitePageId = UrlHelper.getParameterByName('d');
        const backUrl = UrlHelper.getParameterByName('ru');
        this.$rootScope.afterCreatingScenario = { page, sitePageId, backUrl };

        this.$rootScope.tabs = this.$rootScope.tabs || [];
        this.$scope.$emit('onGotoPage', { page: 'create-scenario' });
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            Id: {
                id: 'drpScenarioId',
                required: true,
            },
        });
    }

    //#endregion
}