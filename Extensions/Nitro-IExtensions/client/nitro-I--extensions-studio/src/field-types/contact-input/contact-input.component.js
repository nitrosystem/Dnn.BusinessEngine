import template from './contact-input.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class ContactInputFieldController {
    constructor($scope, $timeout) {
        'ngInject';

        $scope.$on('onBindFieldSettings_ContactInput', (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;

            // Defaults for a new field, so the radio buttons start with a selection
            // (the runtime in contact-input.js uses the same defaults when a value is missing)
            const settings = this.field.Settings || (this.field.Settings = {});
            if (!settings.Mode) settings.Mode = 'both';
            if (!settings.DefaultMethod) settings.DefaultMethod = 'mobile';
        });
    }

    $onInit() {
    }
}

const ContactInputFieldComponent = {
    bindings: {
        field: '<',
        actions: '<',
    },
    controller: ContactInputFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default ContactInputFieldComponent;
