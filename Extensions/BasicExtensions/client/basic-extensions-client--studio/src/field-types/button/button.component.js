import template from './button.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class ButtonFieldController {
    constructor($scope) {
        'ngInject';

        this.$scope = $scope;

        this.$scope.$on('onBindFieldSettings_Button', (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;
        });
    }

    $onInit() {
    }
}

const ButtonFieldComponent = {
    bindings: {
        field: '<',
    },
    controller: ButtonFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default ButtonFieldComponent;