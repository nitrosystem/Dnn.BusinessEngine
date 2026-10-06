import template from './timer-text.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class TimerTextFieldController {
  constructor($scope) {
    'ngInject';

    this.$scope = $scope;

    $scope.$on('onBindFieldSettings_TimerText', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() {
  }
}

const TimerTextFieldComponent = {
  bindings: {
    field: '<',
  },
  controller: TimerTextFieldController,
  controllerAs: '$',
  templateUrl: template,
};

export default TimerTextFieldComponent;
