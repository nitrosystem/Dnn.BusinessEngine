import template from './service-params.html';

class ServiceParamsController {
    constructor($scope) {
        ('ngInject');

        this.$scope = $scope;
    }

    $onInit() {
        this.service = this.service ?? { Params: [] };
    }

    onAddServiceParamClick() {
        this.service.Params = this.service.Params || [];
        this.service.Params.push({ ViewOrder: this.service.Params.length + 1 });
    }
}

const ServiceParamsComponent = {
    bindings: {
        service: '<',
        hideParamType: '<'
    },
    controller: ServiceParamsController,
    controllerAs: '$',
    templateUrl: template
};

export default ServiceParamsComponent;