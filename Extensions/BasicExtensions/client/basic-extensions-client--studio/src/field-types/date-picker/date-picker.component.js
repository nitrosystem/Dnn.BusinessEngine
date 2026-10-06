import template from './date-picker.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class DatePickerFieldController {
  constructor($scope) {
    'ngInject';

    this.$scope = $scope;

    $scope.$on('onBindFieldSettings_DatePicker', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() {
  }
}

const DatePickerFieldComponent = {
  bindings: {
    field: '<',
    actions: '<',
  },
  controller: DatePickerFieldController,
  controllerAs: '$',
  templateUrl: template,
};

export default DatePickerFieldComponent;
