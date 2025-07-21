import template from "./call-action.html";

class CallActionController {
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
  }

  $onInit() {
    this.onPageLoad();
  }

  onPageLoad() {}
}

const CallAction = {
  bindings: {
    action: "<",
    scenarios: "<",
    actions: "<",
    moduleActions: "<",
    services: "<",
    variables: "<",
  },
  controller: CallActionController,
  controllerAs: "$",
  templateUrl: template,
};

export default CallAction;
