import template from './approve-user.html';

class ApproveUserServiceController {
  constructor(
    $scope
  ) {
    'ngInject';

    this.$scope = $scope;

    $scope.$on('onValidateService_ApproveUser', (e, defer) => {
      defer.resolve(true);
    });
  }

  //#region Event Methods

  $onInit() {
  }

  //#endregion
}

const ApproveUserService = {
  bindings: {
    service: '<',
  },
  controller: ApproveUserServiceController,
  controllerAs: '$',
  templateUrl: template,
};

export default ApproveUserService;
