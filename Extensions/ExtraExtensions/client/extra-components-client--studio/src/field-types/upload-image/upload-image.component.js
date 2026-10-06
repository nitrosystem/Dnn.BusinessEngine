import template from "./upload-image.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class UploadImageFieldController {
  constructor($scope) {
    "ngInject";

    this.$scope = $scope;

    $scope.$on('onBindFieldSettings_UploadImage', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() {
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
