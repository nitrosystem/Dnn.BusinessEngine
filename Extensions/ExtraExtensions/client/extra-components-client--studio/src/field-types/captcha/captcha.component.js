import template from './captcha.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class CaptchaFieldController {
    constructor($scope) {
        'ngInject';

        this.$scope = $scope;

        $scope.$on('onBindFieldSettings_Captcha', (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;
        });
    }

    $onInit() {
    }
}

const CaptchaFieldComponent = {
    bindings: {
        field: '<',
    },
    controller: CaptchaFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default CaptchaFieldComponent;