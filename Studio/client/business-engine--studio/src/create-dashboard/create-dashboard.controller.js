import basicOptionsTemplate from "./1-basic-options/basic-options.html";
import pagesTemplate from "./2-pages/pages.html";

export class CreateDashboardController {
    constructor($scope, $rootScope, $timeout, $deferredBroadcast, studioService, globalService) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.$deferredBroadcast = $deferredBroadcast;
        this.globalService = globalService;

        this.basicOptionsTemplate = basicOptionsTemplate;
        this.pagesTemplate = pagesTemplate;

        $scope.$on('onCreateDashboardChangeStep', (e, args) => {
            this.onStepClick(args.step, true);
        });

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        let step = parseInt(this.globalService.getParameterByName('st') || '1');
        this.$rootScope.createDashboardValidatedStep = [step];
        this.$timeout(() => this.onStepClick(step));

        this.onFocusModule();
    }

    onFocusModule() {
        let moduleId = this.globalService.getParameterByName('id');

        this.$rootScope.explorerExpandedItems.push(...["dashboards"]);
        if (moduleId) this.$rootScope.explorerExpandedItems.push(moduleId);

        if (moduleId) this.$rootScope.explorerCurrentItem = moduleId;
    }

    onStepClick(step, isStepClick) {
        if (!isStepClick)
            this.step = step;
        else {
            if (step > (this.step ?? 1)) {
                this.hideStepPreloader = false;

                this.$deferredBroadcast(this.$scope, 'onCreateDashboardValidateStep' + (this.step || 1)).then((isValid) => {
                    if (isValid) {
                        this.hideStepPreloader = false;
                        this.step = step;
                        this.$rootScope.createDashboardValidatedStep.push(step);

                        this.setStepUrl();
                    }
                });
            }
            else if (!this.step || step < (this.step ?? 1)) {
                this.step = step;
                this.$rootScope.createDashboardValidatedStep.push(step);
                this.setStepUrl();
            }
        }
    }

    setStepUrl() {
        let newUrl = this.globalService.replaceUrlParam("st", this.step);
        this.globalService.pushState(newUrl);
    }
}