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

    onShowLoadingConditionsClick() {
        this.field.Settings.LoadingConditions =
            this.field.Settings.LoadingConditions || [];

        window["wnButtonLoadingConditions" + this.field.FieldName].show();
    }

    onAddFieldActionClick() {
        this.$scope.$emit("onGotoPage", {
            page: "create-action",
            parentID: this.field.FieldID,
            subParams: { type: "field" },
        });
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