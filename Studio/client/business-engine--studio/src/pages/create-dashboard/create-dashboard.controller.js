import { UrlHelper } from '../../shared/helpers';
import basicOptionsTemplate from './1-basic-options/basic-options.html';
import pagesTemplate from './2-pages/pages.html';

export class CreateDashboardController {
    constructor($scope, $rootScope, $timeout, $q) {
        'ngInject';

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.$q = $q;

        this.basicOptionsTemplate = basicOptionsTemplate;
        this.pagesTemplate = pagesTemplate;

        const currentTabKey = this.$rootScope.currentTab.key;
        $scope.$on(`onTab--${currentTabKey}--Selected`, (e, args) => {
            this.onFocusModule();
        });

        $scope.createDashboardValidatedStep = [];
        $scope.$on('onCreateDashboardChangeStep', (e, args) => {
            this._gotoStep(args.step);
        });
        

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');
        this.dashboard = { Id: id };

        let step = parseInt(UrlHelper.getParameterByName('st') || '1');
        this.$rootScope.createDashboardValidatedStep = [step];
        this.$timeout(() => this.onStepClick(step));

        this.onFocusModule();
    }

    onFocusModule() {
        let moduleId = UrlHelper.getParameterByName('id');

        this.$rootScope.explorerExpandedItems.push(...['dashboards']);
        if (moduleId) this.$rootScope.explorerExpandedItems.push(moduleId);

        if (moduleId) this.$rootScope.explorerCurrentItem = moduleId;
    }

    onStepClick(step) {
        this._gotoStep(step)
    }

    //#endregion

    //#region Private Methods

    _gotoStep(step) {
        if (step === this.step) return;

        const raise = (step) => {
            if (this.step === step) {
                this.$scope.createDashboardValidatedStep.push(this.step);
                this._setStepUrl();
            }
        }
        
        if (this.step < step) {
            const defer = this.$q.defer();
            this.$scope.$broadcast('onCreateDashboardValidateStep' + this.step, defer);
            defer.promise.then((isValid) => {
                if (isValid) {
                    this.step = step;
                    raise(step);
                }
            });
        }
        else if (!this.step || this.step > step) {
            this.step = step;
            raise(step);
        }
    }


    _setStepUrl() {
        let newUrl = UrlHelper.replaceUrlParam('st', this.step);
        UrlHelper.pushState(newUrl);
    }

    //#endregion
}