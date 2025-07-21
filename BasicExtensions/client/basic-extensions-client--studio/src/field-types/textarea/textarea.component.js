import template from "./textarea.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class TextareaFieldController {
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

const TextareaFieldComponent = {
  bindings: {
    field: "<",
  },
  controller: TextareaFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default TextareaFieldComponent;
