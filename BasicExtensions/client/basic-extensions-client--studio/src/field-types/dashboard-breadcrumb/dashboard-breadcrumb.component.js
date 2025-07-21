import template from "./dashboard-breadcrumb.html";

class DashboardBreadcrumbFieldController {
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

  onSetTemplateClick() {
    this.workingMode = "edit-template";
    this.$scope.$emit("onShowRightWidget");
  }

  onSaveTemplateClick() {
    this.disposeWorkingMode();
  }

  onCancelEditTemplateClick() {
    this.disposeWorkingMode();
  }

  disposeWorkingMode() {
    this.$scope.$emit("onHideRightWidget");

    this.$timeout(() => {
      delete this.workingMode;
    }, 200);
  }
}

const DashboardBreadcrumbFieldComponent = {
  bindings: {
    field: "<",
  },
  controller: DashboardBreadcrumbFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default DashboardBreadcrumbFieldComponent;
