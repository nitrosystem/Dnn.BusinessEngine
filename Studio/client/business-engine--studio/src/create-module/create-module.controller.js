import basicOptionsTemplate from "./1-basic-options/basic-options.html";
import templateTemplate from "./2-template/template.html";
import librariesTemplate from "./3-libraries/libraries.html";
import variablesTemplate from "./4-variables/variables.html";
import moduleBuilderTemplate from "./5-module-builder/module-builder.html";
import actionsTemplate from "./6-actions/actions.html";

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
        this.librariesTemplate = librariesTemplate;
        this.variablesTemplate = variablesTemplate;
        this.moduleBuilderTemplate = moduleBuilderTemplate;
        this.actionsTemplate = actionsTemplate;

        this.$rootScope.createModuleValidatedStep = [];

        $scope.$on('onCreateModuleChangeStep', (e, args) => {
            this.gotoStep(args.step);
        });

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");
        const step = this.globalService.getParameterByName("st");

        this.step = id
            ? parseInt(step) || 1
            : 1;
        this.gotoStep(this.step);

        this.onFocusModule();
    }

    onFocusModule() {
        let moduleId = this.globalService.getParameterByName('id');

        this.$rootScope.explorerExpandedItems.push(...["modules", "create-module"]);
        if (moduleId) this.$rootScope.explorerExpandedItems.push(moduleId);

        this.$rootScope.explorerCurrentItem = !moduleId ? "create-module" : moduleId;
    }

    onStepClick(step) {
        this.gotoStep(step)
    }

    gotoStep(step) {
        if (this.step < step) {
            this.$deferredBroadcast(this.$scope, 'onCreateModuleValidateStep' + this.step).then((isValid) => {
                this.step = isValid ? step : this.step;
                this.$rootScope.createModuleValidatedStep.push(this.step);
                this.setStepUrl();
            });
        }
        else if (this.step > step) {
            this.step = step;
        }

        if (this.step == step) {
            this.$rootScope.createModuleValidatedStep.push(this.step);
            this.setStepUrl();
        }
    }

    setStepUrl() {
        let newUrl = this.globalService.replaceUrlParam("st", this.step);
        this.globalService.pushState(newUrl);
    }
}