import template from "./dropdown-list.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class DropdownListFieldController {
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

const DropdownListFieldComponent = {
    bindings: {
        field: "<",
    },
    controller: DropdownListFieldController,
    controllerAs: "$",
    templateUrl: template,
};

export default DropdownListFieldComponent;