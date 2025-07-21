import template from "./send-email.html";

class SendEmailServiceController {
    constructor(
        $scope,
        $q,
        $timeout,
        $deferredEmit,
        globalService,
        apiService,
        validationService
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$q = $q;
        this.$timeout = $timeout;
        this.$deferredEmit = $deferredEmit;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;

        $scope.$on("onValidateService", (e, task, args) => {
            this.validateService.apply(this, [task, args]);
        });
    }

    $onInit() {
        this.onPageLoad();
    }

    onPageLoad() {
        this.setForm();
    }

    setForm() {
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
            "$.service.Settings"
        );
    }

    validateService(task, args) {
        task.wait(() => {
            var defer = this.$q.defer();

            this.form.validated = true;
            this.form.validator(this.service);
            if (this.form.valid || 1 == 1) defer.resolve(true);

            return defer.promise;
        });
    }
}

const SendEmailService = {
    bindings: {
        service: "<",
    },
    controller: SendEmailServiceController,
    controllerAs: "$",
    templateUrl: template,
};

export default SendEmailService;