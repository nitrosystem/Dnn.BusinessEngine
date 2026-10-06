import template from './submit-entity.html';
import { baseQuery_insert, baseQuery_insertupdate, baseQuery_update } from './sql-query-template.js';
import { Global } from '../../shared/global.js';

class SubmitEntityServiceController {
    constructor($scope, $rootScope, validationService) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.validationService = validationService;

        this.baseQueryInsertTemplate = baseQuery_insert;
        this.baseQueryInsertUpdateTemplate = baseQuery_insertupdate;
        this.baseQueryUpdateTemplate = baseQuery_update;

        $scope.$on('onValidateService_SubmitEntity', (e, defer) => {
            this._validateService(defer);
        });

        const serviceNameWatch = $scope.$watch('$.service.ServiceName', (newVal, oldVal) => {
            if (newVal !== oldVal && !this.submitEntityService.Settings.StoredProcedureNameModified)
                this.submitEntityService.Settings.StoredProcedurePostfixName = newVal;

            if (this.submitEntityService.Settings.StoredProcedureNameModified)
                serviceNameWatch();
        });
    }

    //#region Event Methods

    $onInit() {
        this.onPageLoad();
    }

    onPageLoad() {
        this.entities = this.serviceController.extensionDependency.Entities;
        this.submitEntityService = this.serviceController.extensionService ??
        {
            ActionType: 0,
            Settings: {
                StoredProcedurePrefixName: this.$rootScope.scenario.DatabaseObjectPrefix,
                ...(this.service && { StoredProcedurePostfixName: this.service.ServiceName })
            }
        };

        this.onActionTypeChange();

        if (this.submitEntityService.EntityId)
            this.entity = this.entities.find(e => e.Id === this.submitEntityService.EntityId);

        this._setForm();
    }

    onResetBaseQueryClick() {
        this.submitEntityService.BaseQuery = '';
        this.onActionTypeChange();
    }

    onActionTypeChange(resetBaseQuery) {
        if (!this.submitEntityService.BaseQuery || resetBaseQuery) {
            switch (this.submitEntityService.ActionType) {
                case 0:
                    this.submitEntityService.BaseQuery = this.baseQueryInsertUpdateTemplate;
                    break;
                case 1:
                    this.submitEntityService.BaseQuery = this.baseQueryInsertTemplate;
                    break;
                case 2:
                    this.submitEntityService.BaseQuery = this.baseQueryUpdateTemplate;
                    break;
            }
        }
    }

    onSelectedEntityChange() {
        const entity = this.entities.find(e => e.Id === this.submitEntityService.EntityId);
        if (!entity) return;

        this.entity = entity;
        this.submitEntityService.Entity = entity;
        this.submitEntityService.Entity.InsertColumns = [];
        this.submitEntityService.Entity.UpdateColumns = [];
        this.service.Params = this.service.Params ?? [];

        let index = 1;
        entity.Columns?.forEach(c => {
            const paramName = `@${c.ColumnName}`;
            if (!this.service.Params.some(p => p.ParamName === paramName)) {
                this.service.Params.push({
                    ParamName: paramName,
                    ParamType: c.ColumnType,
                    ViewOrder: index++
                });
            }
        });

        const primaryColumn = entity.Columns?.find(c => c.IsPrimary);
        if (primaryColumn) {
            const colName = primaryColumn.ColumnName;
            this.submitEntityService.Entity.PrimaryKeyParam = `@${colName}`;
            this.submitEntityService.Entity.InsertConditions = [{
                SqlQuery: `NOT EXISTS(\n\t\tSELECT [${colName}] FROM {Schema}.{TableName} \n\t\tWHERE [${colName}] = @${colName}\n\t)`,
                GroupName: 'Group1'
            }];
            this.submitEntityService.Entity.UpdateConditions = [{
                SqlQuery: `[${colName}] = @${colName}`,
                GroupName: 'Group1'
            }];
        }

        entity.Columns?.forEach(c => {
            if (!c.IsIdentity) {
                this.submitEntityService.Entity.InsertColumns.push(Global.clone(c));
                this.submitEntityService.Entity.UpdateColumns.push(Global.clone(c));
            }
        });
    }

    onUpdateEntityColumnsClick() {
        const entity = this.entities.find(e => e.Id === this.submitEntityService.EntityId);
        if (!entity) return;

        const sourceColumns = entity.Columns?.filter(c => !c.IsIdentity) ?? [];
        const actionType = this.submitEntityService.ActionType;
        if (actionType === 0 || actionType === 1)
            this.submitEntityService.Entity.InsertColumns =
                this._mergeColumns(sourceColumns, this.submitEntityService.Entity.InsertColumns ?? []);

        if (actionType === 0 || actionType === 2)
            this.submitEntityService.Entity.UpdateColumns =
                this._mergeColumns(sourceColumns, this.submitEntityService.Entity.UpdateColumns ?? []);
    }

    onSelectedInsertEntityAllColumnsChange() {
        this.submitEntityService.Entity.InsertColumns?.forEach(c => {
            c.IsSelected = !c.IsSelected;
            this.onSelectedInsertEntityColumnChange(c);
        });
    }

    onSelectedInsertEntityColumnChange(c) {
        this._applyColumnValue(c);
    }

    onSelectedUpdateEntityAllColumnsChange() {
        this.submitEntityService.Entity.UpdateColumns?.forEach(c => {
            c.IsSelected = !c.IsSelected;
            this.onSelectedUpdateEntityColumnChange(c);
        });
    }

    onSelectedUpdateEntityColumnChange(c) {
        this._applyColumnValue(c);
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            ActionType: {
                required: true,
            },
            EntityId: {
                id: 'drpEntityId',
                required: true,
            },
            Entity: {
                required: true,
            },
            'Entity.InsertColumns': {
                rule: (value) => {
                    const actionType = this.submitEntityService.ActionType;
                    if ((actionType === 0 || actionType === 1) &&
                        !(value || []).some(c => c.IsSelected))
                        return 'Select column(s) for insert query.';
                    return true;
                },
            },
            'Entity.UpdateColumns': {
                rule: (value) => {
                    const actionType = this.submitEntityService.ActionType;
                    if ((actionType === 0 || actionType === 2) &&
                        !(value || []).some(c => c.IsSelected))
                        return 'Select column(s) for update query.';
                    return true;
                },
            },
            'Settings.StoredProcedurePostfixName': {
                id: 'txtSpPostfix',
                required: true,
            },
        },
            true,
            this.$scope,
            '$.submitEntityService'
        );
    }

    _mergeColumns(sourceColumns, destColumns) {
        const sourceMap = Object.fromEntries(sourceColumns.map(c => [c.Id, c]));
        const destMap = Object.fromEntries(destColumns.map(c => [c.Id, c]));
        const updatedColumns = destColumns
            .map(col => sourceMap[col.Id] ? { ...col, ...sourceMap[col.Id] } : null)
            .filter(Boolean);
        const newColumns = sourceColumns.filter(col => !destMap[col.Id]);
        return [...updatedColumns, ...newColumns];
    }

    _applyColumnValue(c) {
        const paramName = `@${c.ColumnName}`;
        if (c.IsSelected && this.service.Params?.some(p => p.ParamName === paramName))
            c.ColumnValue = paramName;
    }

    _validateService(defer) {
        this.form.validated = true;
        this.form.validator(this.submitEntityService);
        if (this.form.valid) {
            if (!this.submitEntityService.Settings.StoredProcedureNameModified)
                this.submitEntityService.StoredProcedureName =
                    this.submitEntityService.Settings.StoredProcedurePrefixName +
                    this.submitEntityService.Settings.StoredProcedurePostfixName;

            this.serviceController.extensionService = this.submitEntityService;
        }

        defer.resolve(this.form.valid);
    }

    //#endregion
}

const SubmitEntityService = {
    bindings: {
        serviceController: '<',
        service: '<'
    },
    controller: SubmitEntityServiceController,
    controllerAs: '$',
    templateUrl: template,
};

export default SubmitEntityService;
