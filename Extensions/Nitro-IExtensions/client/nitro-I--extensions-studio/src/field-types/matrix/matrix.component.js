import template from "./matrix.html";
import sidebarSettingsTemplate from "./sidebar-settings.html";
import editColumnWidget from "./edit-column.html";

class MatrixFieldController {
    constructor($scope, $timeout, globalService) {
        "ngInject";

        this.$scope = $scope;
        this.$timeout = $timeout;
        this.globalService = globalService;

        this.editColumnWidget = editColumnWidget;
    }

    $onInit() {
        this.$scope.$on(
            "onBindFieldSettings_" + this.field.FieldName,
            (e, args) => {
                this.field.CustomSettings = sidebarSettingsTemplate;
            }
        );
    }

    onAddColumnClick() {
        this.workingMode = "edit-column";
        this.$scope.$emit("onShowRightWidget");

        this.column = { IsNew: true };
    }

    onEditColumnClick(column) {
        this.columnBackup = column;
        this.column = _.clone(column);

        this.workingMode = "edit-column";
        this.$scope.$emit("onShowRightWidget");
    }

    onDeleteColumnClick(column) {
        if (confirm("Are you sure for delete this column!?")) {
            this.field.Settings.Columns.splice(
                this.field.Settings.Columns.indexOf(column),
                1
            );
        }
    }

    onAddContentItemClick() {
        this.column.ContentItems = this.column.ContentItems || [];
        this.column.ContentItems.push({});
    }

    onShowContentItemConditionsClick(item) {
        this.contentItem = item;

        window["wnContentItemConditions"].show();
    }

    onSaveContentItemConditionsClick() {
        window["wnContentItemConditions"].hide();
    }

    onColumnShowConditionsClick() {
        window["wnColumnShowConditions"].show();
    }

    onActionChange() {
        this.column.ActionParams = this.column.ActionParams || [];
    }

    onAddActionItemClick() {
        this.column.ActionItems = this.column.ActionItems || [];
        this.column.ActionItems.push({});
    }

    onSetActionParamsClick(item) {
        const action = _.find(this.actions, (a) => {
            return a.ActionID == item.ActionID;
        });

        item.ActionParams = _.assign(
            _.cloneDeep(action.Params),
            item.ActionParams || []
        );
        this.$scope.$broadcast("onReinitActionParams");

        this.actionItem = item;

        window["wnActionParams"].show();
    }

    onSaveColumnClick() {
        this.field.Settings.Columns = this.field.Settings.Columns || [];

        if (this.column.IsNew) {
            delete this.column.IsNew;
            this.field.Settings.Columns.push(this.column);
        } else {
            this.field.Settings.Columns[
                this.field.Settings.Columns.indexOf(this.columnBackup)
            ] = this.column;
        }

        this.disposeWorkingMode();
    }

    onCancelColumnClick() {
        this.field.Settings.Columns[
            this.field.Settings.Columns.indexOf(this.columnBackup)
        ] = this.columnBackup;

        this.disposeWorkingMode();
    }

    onEditFieldDataSourceClick() {
        this.$scope.$emit("onShowFieldDataSource", { field: this.field });
    }

    disposeWorkingMode() {
        this.$scope.$emit("onHideRightWidget");

        this.$timeout(() => {
            delete this.workingMode;
        }, 200);
    }
}

const MatrixFieldComponent = {
    bindings: {
        field: "<",
        actions: "<",
    },
    controller: MatrixFieldController,
    controllerAs: "$",
    templateUrl: template,
};

export default MatrixFieldComponent;