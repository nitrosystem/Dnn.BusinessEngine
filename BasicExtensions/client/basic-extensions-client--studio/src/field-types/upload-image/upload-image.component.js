import template from "./upload-image.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class UploadImageFieldController {
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

const UploadImageFieldComponent = {
  bindings: {
    field: "<",
  },
  controller: UploadImageFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default UploadImageFieldComponent;
