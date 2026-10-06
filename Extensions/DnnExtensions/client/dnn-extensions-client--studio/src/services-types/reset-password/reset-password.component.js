import template from './reset-password.html';

class ResetPasswordServiceController {
  constructor(
    $scope,
    $q,
    $timeout,
    $deferredEmit,
    globalService,
    apiService,
    validationService
  ) {
    'ngInject';

    this.$scope = $scope;
    this.$q = $q;
    this.$timeout = $timeout;
    this.$deferredEmit = $deferredEmit;
    this.globalService = globalService;
    this.apiService = apiService;
    this.validationService = validationService;

    $scope.$on('onValidateService_ResetPassword', (e, defer) => {
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

const ResetPasswordService = {
  bindings: {
    service: '<',
  },
  controller: ResetPasswordServiceController,
  controllerAs: '$',
  templateUrl: template,
};

export default ResetPasswordService;
