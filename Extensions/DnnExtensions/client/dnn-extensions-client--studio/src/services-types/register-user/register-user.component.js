import template from "./register-user.html";

class RegisterUserServiceController {
  constructor(
    $scope,
    validationService
  ) {
    "ngInject";

    this.$scope = $scope;
    this.validationService = validationService;

    $scope.$on("onValidateService_RegisterUser", (e, defer) => {
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
        RepeatPassword: {
          required: true,
        },
      },
      true,
      this.$scope,
      "$.service.Settings"
    );
  }

  _validateService(defer) {
    this.form.validated = true;
    this.form.validator(this.service.Settings);
    defer.resolve(this.form.valid);
  }

  //#endregion
}

const RegisterUserService = {
  bindings: {
    service: "<",
  },
  controller: RegisterUserServiceController,
  controllerAs: "$",
  templateUrl: template,
};

export default RegisterUserService;
