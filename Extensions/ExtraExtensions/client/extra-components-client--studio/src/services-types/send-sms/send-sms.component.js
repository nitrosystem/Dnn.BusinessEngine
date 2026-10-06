import template from './send-sms.html';

class SendSmsServiceController {
  constructor(
    $scope,
    validationService
  ) {
    'ngInject';

    this.$scope = $scope;
    this.validationService = validationService;

    $scope.$on('onValidateService_SendSms', (e, defer) => {
      this._validateService(defer);
    });
  }

  //#region Event Methods

  $onInit() {
    this.onPageLoad();
  }

  onPageLoad() {
    this.setForm();
  }

  onAddParamsClick() {
    this.service.Settings.Parameters = this.service.Settings.Parameters || [];
    this.service.Settings.Parameters.push({});
  }

  //#endregion

  //#region Private Methods

  _setForm() {
    this.form = this.validationService.init(
      {
        Provider: {
          required: true,
        },
        Message: {
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
    this.form.validator(this.service);
    defer.resolve(this.form.valid);
  }
}

const SendSmsService = {
  bindings: {
    service: '<',
  },
  controller: SendSmsServiceController,
  controllerAs: '$',
  templateUrl: template,
};

export default SendSmsService;
