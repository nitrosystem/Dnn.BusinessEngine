import template from "./dashboard-link.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class DashboardLinkFieldController {
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

const DashboardLinkFieldComponent = {
  bindings: {
    field: "<",
  },
  controller: DashboardLinkFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default DashboardLinkFieldComponent;
