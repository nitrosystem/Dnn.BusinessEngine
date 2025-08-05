import template from "./run-service.component.html";
import _ from "lodash";

class RunServiceActionController {
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

const RunServiceAction = {
    bindings: {
        controller: "<",
        action: "<",
        scenarios: "<",
        actions: "<",
        services: "<",
        serviceType: "@",
        datasource: "@",
        variables: "<",
        fields: "<",
        appModels: "<",
    },
    controller: RunServiceActionController,
    controllerAs: "$",
    templateUrl: template,
};

export default RunServiceAction;