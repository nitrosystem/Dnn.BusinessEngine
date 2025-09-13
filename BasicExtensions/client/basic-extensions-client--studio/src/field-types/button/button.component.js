import template from "./button.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";

class ButtonFieldController {
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

    onAddFieldActionClick() {
        const page = {
            page: "create-action",
            subParams: {
                module: this.field.ModuleId,
                type: this.field.FieldType,
                field: this.field.Id
            }
        };

        this.$scope.$emit("onGotoPage", page);
    }
}

const ButtonFieldComponent = {
    bindings: {
        field: "<",
    },
    controller: ButtonFieldController,
    controllerAs: "$",
    templateUrl: template,
};

export default ButtonFieldComponent;