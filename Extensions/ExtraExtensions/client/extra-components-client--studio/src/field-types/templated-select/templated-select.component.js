import template from './templated-select.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class TemplatedSelectFieldController {
    constructor($scope) {
        'ngInject';

        this.$scope = $scope;

        $scope.$on('onBindFieldSettings_TemplatedSelect', (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;
        });
    }

    $onInit() {
    }
}

const TemplatedSelectFieldComponent = {
    bindings: {
        field: '<',
    },
    controller: TemplatedSelectFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default TemplatedSelectFieldComponent;