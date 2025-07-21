import template from "./upload-file.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class UploadFileFieldController {
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

const UploadFileFieldComponent = {
  bindings: {
    field: "<",
  },
  controller: UploadFileFieldController,
  controllerAs: "$",
  templateUrl: template,
};

export default UploadFileFieldComponent;
