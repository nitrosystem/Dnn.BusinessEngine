import template from './slimselect-dropdown.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class SlimSelectDropdownFieldController {
    constructor($scope) {
        'ngInject';

        this.$scope = $scope;

        $scope.$on('onBindFieldSettings_SlimSelectDropdown', (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;
        });
    }

    $onInit() {
    }
}

const SlimSelectDropdownFieldComponent = {
    bindings: {
        field: '<',
    },
    controller: SlimSelectDropdownFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default SlimSelectDropdownFieldComponent;