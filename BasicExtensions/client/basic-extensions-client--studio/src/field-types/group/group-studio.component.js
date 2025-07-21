import template from "./group-studio.component.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class GroupFieldController {
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

const GroupFieldComponent = {
  bindings: {
    field: "<",
  },
  controller: GroupFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default GroupFieldComponent;
