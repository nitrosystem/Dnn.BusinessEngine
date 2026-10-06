import template from './bootstrap-modal.component.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class BootstrapModalFieldController {
  constructor($scope) {
    'ngInject';

    this.$scope = $scope;

    $scope.$on('onBindFieldSettings_BootstrapModal', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() {
  }
}

const BootstrapModalFieldComponent = {
  bindings: {
    field: '<',
  },
  controller: BootstrapModalFieldController,
  controllerAs: '$',
  templateUrl: template,
};

export default BootstrapModalFieldComponent;
