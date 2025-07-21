import template from "./register-user.html";

class RegisterUserServiceController {
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

  validateService(task, args) {
    task.wait(() => {
      var defer = this.$q.defer();

      this.form.validated = true;
      this.form.validator(this.service.Settings);
      if (this.form.valid) defer.resolve(true);

      return defer.promise;
    });
  }
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
