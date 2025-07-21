import template from "./editor.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class TextEditorFieldController {
  constructor($scope, $timeout) {
    "ngInject";

    this.$scope = $scope;
    this.$timeout = $timeout;
  }

  $onInit() {
    this.$scope.$on(
      "onBindFieldSettings_" + this.field.FieldName,
      (e, args) => {
        this.field.CustomSettings = sidebarSettingsTemplate;
      }
    );
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
