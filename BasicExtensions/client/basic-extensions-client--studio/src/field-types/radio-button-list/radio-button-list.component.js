import template from "./radio-button-list.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class RadioButtonListFieldController {
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

const RadioButtonListFieldComponent = {
    bindings: {
        field: "<",
        actions: "<",
    },
    controller: RadioButtonListFieldController,
    controllerAs: "$",
    templateUrl: template,
};

export default RadioButtonListFieldComponent;