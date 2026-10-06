import template from "./editor.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class TextEditorFieldController {
  constructor($scope) {
    "ngInject";

    this.$scope = $scope;

    $scope.$on('onBindFieldSettings_TextEditor', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() {
  }
}

const TextEditorFieldComponent = {
  bindings: {
    field: "<",
  },
  controller: TextEditorFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default TextEditorFieldComponent;
