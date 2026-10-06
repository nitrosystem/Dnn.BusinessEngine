import template from './update-user.html';

class UpdateUserServiceController {
  constructor(
    $scope,
    validationService
  ) {
    'ngInject';

    this.$scope = $scope;
    this.validationService = validationService;

    $scope.$on('onValidateService_UpdateUser', (e, defer) => {
      this._validateService(defer);
    });
  }

  //#region Event Methods

  $onInit() {
    this.onPageLoad();
  }

  onPageLoad() {
    this._setForm();
  }

  //#endregion

  //#region Private Methods

  _setForm() {
    this.form = this.validationService.init(
      {
        UserID: {
          required: true,
        },
        FirstName: {
          required: true,
        },
        LastName: {
          required: true,
        },
        UserPhoto: {
          required: true,
        },
      },
      true,
      this.$scope,
      '$.service.Settings'
    );
  }

  _validateService(defer) {
    this.form.validated = true;
    this.form.validator(this.service.Settings);
    defer.resolve(this.form.valid);
  }

  //#endregion
}

const UpdateUserService = {
  bindings: {
    service: '<',
  },
  controller: UpdateUserServiceController,
  controllerAs: '$',
  templateUrl: template,
};

export default UpdateUserService;
