import template from './dropdown-list.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class DropdownListFieldController {
    constructor($scope) {
        'ngInject';

        this.$scope = $scope;

        $scope.$on('onBindFieldSettings_DropdownList', (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;
        });
    }

    $onInit() {
    }
}

const DropdownListFieldComponent = {
    bindings: {
        field: '<',
    },
    controller: DropdownListFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default DropdownListFieldComponent;