import template from "./run-service.component.html";
import _ from "lodash";

class DatabaseActionController {
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

const DatabaseAction = {
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
        viewModels: "<",
    },
    controller: DatabaseActionController,
    controllerAs: "$",
    templateUrl: template,
};

export default DatabaseAction;