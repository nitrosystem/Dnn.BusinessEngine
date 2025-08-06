import template from "./service-params.html";

class ServiceParamsController {
    constructor($scope, globalService) {
        ("ngInject");

        this.$scope = $scope;
        this.globalService = globalService;
    }

    $onInit() {
        this.service = this.service ?? { Params: [] };
    }

    onAddServiceParamClick() {
        this.service.Params = this.service.Params || [];
        this.service.Params.push({});
    }
}

const ServiceParamsComponent = {
    bindings: {
        service: "<",
        hideParamType: "<"
    },
    controller: ServiceParamsController,
    controllerAs: "$",
    templateUrl: template
};

export default ServiceParamsComponent;