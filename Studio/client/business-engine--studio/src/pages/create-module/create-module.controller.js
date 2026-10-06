import { UrlHelper } from '../../shared/helpers';
import basicOptionsTemplate from './1-basic-options/basic-options.html';
import templateTemplate from './2-template/template.html';
import librariesTemplate from './3-libraries/libraries.html';
import variablesTemplate from './4-variables/variables.html';
import moduleBuilderTemplate from './5-module-builder/module-builder.html';
import actionsTemplate from './6-actions/actions.html';

export class CreateModuleController {
    constructor($scope, $rootScope, $timeout, $q) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.$q = $q;

        this.basicOptionsTemplate = basicOptionsTemplate;
        this.templateTemplate = templateTemplate;
        this.librariesTemplate = librariesTemplate;
        this.variablesTemplate = variablesTemplate;
        this.moduleBuilderTemplate = moduleBuilderTemplate;
        this.actionsTemplate = actionsTemplate;

        $scope.createModuleValidatedStep = [];
        $scope.$on('onCreateModuleChangeStep', (e, args) => {
            this._gotoStep(args.step);
        });

        const currentTabKey = this.$rootScope.currentTab.key;
        $scope.$on(`onTab--${currentTabKey}--Selected`, (e, args) => {
            this.onFocusModule();
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        const id = UrlHelper.getParameterByName('id');
        this.module = { Id: id };

        let step = UrlHelper.getParameterByName('st');
        step = id
            ? parseInt(step) || 1
            : 1;
        this._gotoStep(step);

        this.onFocusModule();
    }

    onFocusModule() {
        let moduleId = UrlHelper.getParameterByName('id');

        this.$rootScope.explorerExpandedItems.push(...['modules', 'create-module']);
        if (moduleId) this.$rootScope.explorerExpandedItems.push(moduleId);

        this.$rootScope.explorerCurrentItem = !moduleId ? 'create-module' : moduleId;
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
                this.$scope.createModuleValidatedStep.push(this.step);
                this._setStepUrl();
            }
        }
        
        if (this.step < step) {
            const defer = this.$q.defer();
            this.$scope.$broadcast('onCreateModuleValidateStep' + this.step, defer);
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