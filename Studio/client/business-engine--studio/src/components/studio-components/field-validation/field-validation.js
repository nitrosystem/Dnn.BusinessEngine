import template from "./field-validation.component.html";

class FieldValidationController {
    constructor($filter, $scope, $element, apiService, globalService) {
        "ngInject";
    }

    $onInit() {
        debugger
        this.text = this.text || 'This field value is not valid';
    }
}

const FieldValidationComponent = {
    bindings: {
        controller: "<",
        field: "<",
        showExample: "<",
        rule: "@",
        text: "@",
    },
    controller: FieldValidationController,
    controllerAs: "$",
    templateUrl: template,
};

export default FieldValidationComponent;
