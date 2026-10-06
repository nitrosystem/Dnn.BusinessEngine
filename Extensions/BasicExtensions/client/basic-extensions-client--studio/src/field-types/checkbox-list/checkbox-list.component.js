import template from './checkbox-list.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class CheckboxListFieldController {
  constructor($scope) {
    'ngInject';

    this.$scope = $scope;

    $scope.$on('onBindFieldSettings_CheckboxList', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() {
  }
}

const CheckboxListFieldComponent = {
  bindings: {
    field: '<',
  },
  controller: CheckboxListFieldController,
  controllerAs: '$',
  templateUrl: template,
};

export default CheckboxListFieldComponent;
