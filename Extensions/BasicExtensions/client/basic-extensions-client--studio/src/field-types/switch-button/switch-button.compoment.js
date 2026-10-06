import template from './switch-button.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class SwitchButtonFieldController {
  constructor($scope) {
    'ngInject';

    this.$scope = $scope;

    $scope.$on('onBindFieldSettings_SwitchButton', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() {
  }
}

const SwitchButtonComponent = {
  bindings: {
    field: '<',
  },
  controller: SwitchButtonFieldController,
  controllerAs: '$',
  templateUrl: template,
};

export default SwitchButtonComponent;
