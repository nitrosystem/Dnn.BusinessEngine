import template from './radio-button-list.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class RadioButtonListFieldController {
    constructor($scope) {
        'ngInject';

        this.$scope = $scope;

        $scope.$on('onBindFieldSettings_RadioButtonList', (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;
        });
    }

    $onInit() {
    }
}

const RadioButtonListFieldComponent = {
    bindings: {
        field: '<',
        actions: '<',
    },
    controller: RadioButtonListFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default RadioButtonListFieldComponent;