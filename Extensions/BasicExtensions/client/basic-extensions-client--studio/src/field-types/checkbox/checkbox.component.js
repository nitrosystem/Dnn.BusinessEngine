import template from './checkbox.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class CheckboxFieldController {
    constructor($scope) {
        'ngInject';

        this.$scope = $scope;

        $scope.$on('onBindFieldSettings_Checkbox' , (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;
        });
    }

    $onInit() {
    }
}

const CheckboxFieldComponent = {
    bindings: {
        field: '<',
    },
    controller: CheckboxFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default CheckboxFieldComponent;