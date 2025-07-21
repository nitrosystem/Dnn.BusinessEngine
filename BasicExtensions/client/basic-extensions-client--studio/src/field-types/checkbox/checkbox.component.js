import template from "./checkbox.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class CheckboxFieldController {
    constructor($scope, $timeout) {
        "ngInject";

        this.$scope = $scope;
        this.$timeout = $timeout;
    }

    $onInit() {
        this.$scope.$on(
            "onBindFieldSettings_" + this.field.FieldName,
            (e, args) => {
                this.field.CustomSettings = sidebarSettingsTemplate;
            }
        );
    }
}

const CheckboxFieldComponent = {
    bindings: {
        field: "<",
    },
    controller: CheckboxFieldController,
    controllerAs: "$",
    templateUrl: template,
};

export default CheckboxFieldComponent;