import template from './grid.html';
import sidebarSettingsTemplate from './sidebar-settings.html';
import editColumnWidget from './edit-column.html';
import { Global } from '../../shared/global';

class GridFieldController {
    constructor($scope, $timeout) {
        'ngInject';

        this.$scope = $scope;
        this.$timeout = $timeout;

        this.editColumnWidget = editColumnWidget;

        $scope.$on('onBindFieldSettings_Grid', (e, args) => {
            this.field.CustomSettings = sidebarSettingsTemplate;
        });
    }

    //#region Event Methods

    $onInit() {
    }

    onAddColumnClick() {
        this.workingMode = 'edit-column';
        this.$scope.$emit('onShowRightWidget');
        this.column = { isNew: true };

        this._bindVariableProperties();
    }

    onEditColumnClick(column) {
        this.columnBackup = column;
        this.column = Global.clone(column);
        this.workingMode = 'edit-column';
        this.$scope.$emit('onShowRightWidget');

        this._bindVariableProperties();
    }

    onDeleteColumnClick(column) {
        if (confirm('Are you sure for delete this column!?'))
            this.field.Settings.Columns.splice(this.field.Settings.Columns.indexOf(column), 1);
    }

    onAddContentItemClick() {
        this.column.ContentItems = this.column.ContentItems || [];
        this.column.ContentItems.push({});
    }

    onShowContentItemConditionsClick(item) {
        this.contentItem = item;
        window['wnContentItemConditions'].show();
    }

    onSaveContentItemConditionsClick() {
        window['wnContentItemConditions'].hide();
    }

    onActionChange() {
        this.column.ActionParams = this.column.ActionParams || [];
    }

    onAddActionItemClick() {
        this.column.ActionItems = this.column.ActionItems || [];
        this.column.ActionItems.push({});
    }

    onSetActionParamsClick(item) {
        const action = this.field.Actions?.find(a => a.Id === item.ActionId);
        if (action) {
            item.ActionParams = [...(action.Params ?? []), ...(item.ActionParams ?? [])];
            this.$scope.$broadcast('onReinitActionParams');
            this.actionItem = item;
            window['wnActionParams'].show();
        }
    }

    onSetVisibilityConditionsClick(item) {
        this.contentItem = item;
        window['wnContentItemConditions'].show();
    }

    onSaveColumnClick() {
        this.field.Settings.Columns = this.field.Settings.Columns ?? [];

        if (this.column.isNew) {
            delete this.column.isNew;
            this.field.Settings.Columns.push(this.column);
        } else {
            this.field.Settings.Columns[this.field.Settings.Columns.indexOf(this.columnBackup)] = this.column;
        }

        this.moduleBuilderController.onSaveFieldClick();
        this._disposeWorkingMode();
    }

    onCancelColumnClick() {
        const index = this.field.Settings.Columns?.findIndex(c => c === this.columnBackup);
        if (index >= 0) this.field.Settings.Columns[index] = this.columnBackup;

        this._disposeWorkingMode();
    }

    //#endregion

    //#region Private Methods

    _bindVariableProperties() {
        const context = {};
        this.properties = this.moduleBuilderController.variablesAsDataSource?.find(
            v => v.VariableName === this.field.DataSource?.VariableName)?.Properties;
        this.conditionContext = {
            row: this.properties?.forEach(p => context[p.PropertyName] =
                Global.getDefaultValueByType(p.PropertyType))
        };
    }

    _disposeWorkingMode() {
        this.$scope.$emit('onHideRightWidget');
        this.$timeout(() => this.workingMode = null, 200);
    }

    //#endregion
}

const GridFieldComponent = {
    bindings: {
        field: '<',
        actions: '<',
        moduleBuilderController: '<'
    },
    controller: GridFieldController,
    controllerAs: '$',
    templateUrl: template,
};

export default GridFieldComponent;