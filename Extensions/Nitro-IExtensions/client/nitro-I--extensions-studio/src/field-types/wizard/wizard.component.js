import template from './wizard.html';
import sidebarSettingsTemplate from './sidebar-settings.html';
import editStepWidget from './edit-step.html';
import { Global } from '../../shared/global';

class WizardFieldController {
    constructor($scope, $timeout, $q) {
        'ngInject';

        this.$scope = $scope;
        this.$timeout = $timeout;
        this.$q = $q;
        this.editStepWidget = editStepWidget;

        $scope.$on('onBindFieldSettings_Wizard', (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;
        });
    }

    $onInit() {
        this.field.Settings = this.field.Settings || {};
        this.field.Settings.ValidationMethod = 'bWizard_onValidateMatrix';
    }

    onAddStepClick() {
        this.workingMode = 'edit-step';
        this.$scope.$emit('onShowRightWidget');

        this.step = { IsNew: true };
    }

    onEditStepClick(step) {
        this.stepBackup = step;
        this.step = Global.clone(step);

        this.workingMode = 'edit-step';
        this.$scope.$emit('onShowRightWidget');
    }

    onSaveStepClick() {
        this.field.Settings.Steps = this.field.Settings.Steps || [];

        if (this.step.IsNew) {
            delete this.step.IsNew;
            this.field.Settings.Steps.push(this.step);
        } else {
            this.field.Settings.Steps[
                this.field.Settings.Steps.indexOf(this.stepBackup)
            ] = this.step;
        }

        let newList = [...this.field.Settings.Steps].sort((a, b) => a.ViewOrder - b.ViewOrder);
        this.field.Settings.Steps = Global.clone(newList);

        this.disposeWorkingMode();
    }

    onCancelStepClick() {
        this.field.Settings.Steps[
            this.field.Settings.Steps.indexOf(this.stepBackup)
        ] = this.stepBackup;

        this.disposeWorkingMode();
    }

    disposeWorkingMode() {
        this.$scope.$emit('onHideRightWidget');

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }
}


const WizardFieldComponent = {
    bindings: {
        field: '<',
    },
    controller: WizardFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default WizardFieldComponent;