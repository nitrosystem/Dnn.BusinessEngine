import template from './upload-file.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class UploadFileFieldController {
  constructor($scope) {
    'ngInject';

    this.$scope = $scope;

    $scope.$on('onBindFieldSettings_UploadFile', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() {
  }
}

const UploadFileFieldComponent = {
  bindings: {
    field: '<',
  },
  controller: UploadFileFieldController,
  controllerAs: '$',
  templateUrl: template,
};

export default UploadFileFieldComponent;
