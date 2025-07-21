import template from "./checkbox-list.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class CheckboxListFieldController {
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

const CheckboxListFieldComponent = {
  bindings: {
    field: "<",
  },
  controller: CheckboxListFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default CheckboxListFieldComponent;
