import template from './textarea.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class TextareaFieldController {
  constructor($scope) {
    'ngInject';

    this.$scope = $scope;

    $scope.$on('onBindFieldSettings_Textarea', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() {
  }
}

const TextareaFieldComponent = {
  bindings: {
    field: '<',
  },
  controller: TextareaFieldController,
  controllerAs: '$',
  templateUrl: template,
};

export default TextareaFieldComponent;
