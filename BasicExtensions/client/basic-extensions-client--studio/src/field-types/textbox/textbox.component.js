import template from "./textbox.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class TextboxFieldController {
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

const TextboxFieldComponent = {
  bindings: {
    field: "<",
    actions: "<",
  },
  controller: TextboxFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default TextboxFieldComponent;
