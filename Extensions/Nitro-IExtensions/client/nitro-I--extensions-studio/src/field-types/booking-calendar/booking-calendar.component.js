import template from './booking-calendar.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class BookingCalendarFieldController {
    constructor($scope, $timeout) {
        'ngInject';

        $scope.$on('onBindFieldSettings_BookingCalendar', (e, args) => {
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

const BookingCalendarFieldComponent = {
    bindings: {
        field: '<',
        actions: '<',
    },
    controller: BookingCalendarFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default BookingCalendarFieldComponent;
