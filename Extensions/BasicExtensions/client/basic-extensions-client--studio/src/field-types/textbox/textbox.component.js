import template from './textbox.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class TextboxFieldController {
  constructor($scope) {
    'ngInject';

    $scope.$on('onBindFieldSettings_Textbox', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() {
  }
}

const TextboxFieldComponent = {
  bindings: {
    field: '<',
    actions: '<',
  },
  controller: TextboxFieldController,
  controllerAs: '$',
  templateUrl: template,
};

export default TextboxFieldComponent;
