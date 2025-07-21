import template from "./switch-button.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class SwitchButtonFieldController {
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

const SwitchButtonComponent = {
  bindings: {
    field: "<",
  },
  controller: SwitchButtonFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default SwitchButtonComponent;
