import template from './login-user.html';

class LoginUserServiceController {
  constructor(
    $scope,
    validationService
  ) {
    'ngInject';

    this.$scope = $scope;
    this.validationService = validationService;

    $scope.$on('onValidateService_LoginUser', (e, defer) => {
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
        Username: {
          required: true,
        },
        Password: {
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

const LoginUserService = {
  bindings: {
    service: '<'
  },
  controller: LoginUserServiceController,
  controllerAs: '$',
  templateUrl: template,
};

export default LoginUserService;
