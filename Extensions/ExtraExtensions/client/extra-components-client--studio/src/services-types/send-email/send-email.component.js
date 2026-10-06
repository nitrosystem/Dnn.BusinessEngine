import template from './send-email.html';

class SendEmailServiceController {
    constructor(
        $scope,
        validationService
    ) {
        'ngInject';

        this.$scope = $scope;
        this.validationService = validationService;

        $scope.$on('onValidateService_SendEmail', (e, defer) => {
            this._validateService(defer);
        });
    }

    //#region Event Methods

    $onInit() {
        this.onPageLoad();
    }

    onPageLoad() {
        this.providers = this.serviceController.extensionDependency.Providers;
        this._setForm();
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init(
            {
                Subject: {
                    required: true,
                },
                Body: {
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

const SendEmailService = {
    bindings: {
        serviceController: '<',
        service: '<',
    },
    controller: SendEmailServiceController,
    controllerAs: '$',
    templateUrl: template,
};

export default SendEmailService;