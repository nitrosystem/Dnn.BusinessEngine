import basicOptionsTemplate from "./1-basic-options/basic-options.html";
import templateTemplate from "./2-template/template.html";
import variablesTemplate from "./3-variables/variables.html";
import librariesTemplate from "./4-libraries/libraries.html";
import builderTemplate from "./5-builder/builder.html";
import actionsTemplate from "./6-actions/actions.html";

import moduleBuilderTemplate from "./../module-builder/module-builder.html";

export class CreateModuleController {
    constructor($scope, $rootScope, $timeout, $deferredBroadcast, studioService, globalService) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.$deferredBroadcast = $deferredBroadcast;
        this.globalService = globalService;

        this.basicOptionsTemplate = basicOptionsTemplate;
        this.templateTemplate = templateTemplate;
        this.variablesTemplate = variablesTemplate;
        this.builderTemplate = builderTemplate;
        this.actionsTemplate = actionsTemplate;
        this.librariesTemplate = librariesTemplate;

        this.moduleBuilderTemplate = moduleBuilderTemplate;

        $scope.$on('onCreateModuleChangeStep', (e, args) => {
            this.onStepClick(args.step);
        });

        $scope.$on('onFillBasicModuleOptions', (e, args) => {
            this.module = args.module;

            let step = parseInt(this.globalService.getParameterByName('st') || '1');
            if (step != this.step) {
                this.$rootScope.createModuleValidatedStep.push(step);
                this.$timeout(() => { this.onStepClick(step); });
            }
        });

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        let step = parseInt(this.globalService.getParameterByName('st') || '1');
        this.$rootScope.createModuleValidatedStep = [step];
        this.$timeout(() => this.onStepClick(step));

        this.onFocusModule();
    }

    onFocusModule() {
        let moduleId = this.globalService.getParameterByName('id');

        this.$rootScope.explorerExpandedItems.push(...["modules", "create-module"]);
        if (moduleId) this.$rootScope.explorerExpandedItems.push(moduleId);
        
        this.$rootScope.explorerCurrentItem = !moduleId ? "create-module" : moduleId;
    }

    onStepClick(step) {
        if (step > (this.step ?? 1)) {
            this.hideStepPreloader = false;

            this.$deferredBroadcast(this.$scope, 'onCreateModuleValidateStep' + (this.step || step)).then((isValid) => {
                if (isValid) {
                    this.hideStepPreloader = false;
                    this.step = step;
                    this.$rootScope.createModuleValidatedStep.push(step);

                    this.setStepUrl();
                }
            });
        }
        else if (!this.step || step < (this.step ?? 1)) {
            this.step = step;
            this.$rootScope.createModuleValidatedStep.push(step);
            this.setStepUrl();
        }
    }

    setStepUrl() {
        let newUrl = this.globalService.replaceUrlParam("st", this.step);
        this.globalService.pushState(newUrl);

        if (this.step == 2) this.$rootScope.currentActivityBar = 'builder';
        else this.$rootScope.currentActivityBar = 'explorer'
    }
}