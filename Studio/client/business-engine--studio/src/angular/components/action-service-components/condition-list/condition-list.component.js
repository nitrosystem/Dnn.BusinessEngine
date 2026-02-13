import template from "./condition-list.component.html";

class ConditionListController {
    constructor() {
        "ngInject";
    }

    $onInit() {
    }
}

const ConditionListComponent = {
    require: {
        ngModel: "^ngModel",
    },
    bindings: {
        conditions: "=ngModel",
        suggestions: "<",
    },
    controller: ConditionListController,
    controllerAs: "$",
    templateUrl: template,
    transclude: true,
};

export default ConditionListComponent;