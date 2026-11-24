import template from './content.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class ContentFieldController {
  constructor($scope) {
    'ngInject';

    this.$scope = $scope;
    $scope.$on('onBindFieldSettings_Content', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() { }
}

const ContentFieldComponent = {
  bindings: {
    field: '<',
  },
  controller: ContentFieldController,
  controllerAs: '$',
  templateUrl: template,
};

export default ContentFieldComponent;
