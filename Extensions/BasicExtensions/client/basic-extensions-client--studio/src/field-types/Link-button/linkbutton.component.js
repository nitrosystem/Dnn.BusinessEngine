import template from './link-button.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class LinkButtonFieldController {
    constructor($scope) {
        'ngInject';

        this.$scope = $scope;

        $scope.$on('onBindFieldSettings_LinkButton', (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;
        });
    }

    $onInit() {
    }
}

const LinkButtonFieldComponent = {
    bindings: {
        field: '<',
    },
    controller: LinkButtonFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default LinkButtonFieldComponent;