import template from "./dashboard-menu.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class DashboardMenuFieldController {
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

const DashboardMenuFieldComponent = {
  bindings: {
    field: "<",
  },
  controller: DashboardMenuFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default DashboardMenuFieldComponent;
