import template from "./javascript.component.html";

class JavascriptActionController {
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

const JavascriptAction = {
  bindings: {
    action: "<",
    scenarios: "<",
    actions: "<",
    services: "<",
    variables: "<",
  },
  controller: JavascriptActionController,
  controllerAs: "$",
  templateUrl: template,
};

export default JavascriptAction;
