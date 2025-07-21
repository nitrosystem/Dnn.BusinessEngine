import template from "./set-variable.component.html";

class SetVariableActionController {
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

const SetVariableAction = {
  bindings: {
    action: "<",
    scenarios: "<",
    actions: "<",
    services: "<",
    variables: "<",
  },
  controller: SetVariableActionController,
  controllerAs: "$",
  templateUrl: template,
};

export default SetVariableAction;
