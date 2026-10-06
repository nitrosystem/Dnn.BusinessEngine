import template from './group-studio.component.html';
import sidebarSettingsTemplate from './sidebar-settings.html';

class GroupFieldController {
  constructor($scope) {
    'ngInject';

    this.$scope = $scope;

    $scope.$on('onBindFieldSettings_Group', (e, args) => {
      this.field.CustomSettings = sidebarSettingsTemplate;
    });
  }

  $onInit() {
  }
}

const GroupFieldComponent = {
  bindings: {
    field: '<',
  },
  controller: GroupFieldController,
  controllerAs: '$',
  templateUrl: template,
};

export default GroupFieldComponent;
