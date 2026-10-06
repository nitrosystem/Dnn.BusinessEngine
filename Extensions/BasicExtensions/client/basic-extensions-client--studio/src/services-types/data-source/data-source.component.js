import template from './data-source.html';
import { baseQuery } from './sql-query-template.js';
import { Global } from '../../shared/global.js';

class DataSourceServiceController {
    constructor($rootScope, $scope, $timeout, validationService, sqlConstants) {
        'ngInject';

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.validationService = validationService;
        this.sqlConstants = sqlConstants;

        this.baseQueryTemplate = baseQuery;

        $scope.$on('onValidateService_DataSource', (e, defer) => {
            this._validateService(defer);
        });

        const serviceNameWatch = $scope.$watch('$.service.ServiceName', (newVal, oldVal) => {
            if (newVal !== oldVal && !this.dataSourceService.Settings.StoredProcedureNameModified)
                this.dataSourceService.Settings.StoredProcedurePostfixName = newVal;

            if (this.dataSourceService.Settings.StoredProcedureNameModified)
                serviceNameWatch();
        });
    }

    //#region Event Methods

    $onInit() {
        this.onPageLoad();
    }

    onPageLoad() {
        this.entities = this.serviceController.extensionDependency.Entities;
        this.appModels = this.serviceController.extensionDependency.AppModels;
        this.dataSourceService = this.serviceController.extensionService ??
        {
            BaseQuery: this.baseQueryTemplate,
            Entities: [],
            Filters: [],
            SortItems: [],
            Settings: {
                StoredProcedurePrefixName: this.$rootScope.scenario.DatabaseObjectPrefix,
                ...(this.service && { StoredProcedurePostfixName: this.service.ServiceName })
            },
            EnablePaging: true,
            PageIndexParam: '@PageIndex',
            PageSizeParam: '@PageSize'
        };

        if (!this.service.Id)
            this.service.Params = [
                { ParamName: '@PageIndex', ParamType: 'int', ViewOrder: 1 },
                { ParamName: '@PageSize', ParamType: 'int', ViewOrder: 2 }
            ];

        this.dataSourceService.Entities?.forEach(e => {
            const matched = this.entities.find(ee => ee.Id === e.Id);

            if (matched) e.Columns = matched.Columns;
        });

        this.dataSourceService.ModelProperties?.forEach(prop => this.onSelectedEntityAliasChange(prop));
        this.dataSourceService.SortItems?.forEach(s => this.onSortItemSelectedEntityAliasChange(s));

        this._setForm();
    }

    onResetBaseQueryClick() {
        this.dataSourceService.BaseQuery = this.baseQueryTemplate;
    }

    onSelectedAppModelChange() {
        this.onRefreshAppModelClick();
    }

    onRefreshAppModelClick() {
        const result = [];
        const appModel = this.appModels.find(v => v.Id === this.dataSourceService.AppModelId);
        if (!appModel) {
            this.dataSourceService.ModelProperties = result;
            return;
        }

        appModel.Properties?.forEach(prop => {
            const property = {
                Id: prop.Id,
                PropertyName: prop.PropertyName,
                ValueType: 'DataSource'
            };

            const existing = this.dataSourceService.ModelProperties?.find(p => p.Id === prop.Id);
            if (existing) {
                property.IsSelected = existing.IsSelected;
                property.ValueType = existing.ValueType;
                property.EntityAliasName = existing.EntityAliasName;
                property.ColumnName = existing.ColumnName;
                property.Value = existing.Value;
            }

            if (!property.IsSelected && !property.Value) {
                this.dataSourceService.Entities?.forEach(entity => {
                    const column = entity.Columns?.find(c => c.ColumnName === property.PropertyName);
                    if (column) {
                        property.IsSelected = true;
                        property.ValueType = 'DataSource';
                        property.EntityAliasName = entity.AliasName;
                        property.Columns = entity.Columns;
                        property.ColumnName = column.ColumnName;
                    }
                });
            }

            result.push(property);
        });

        this.dataSourceService.ModelProperties = result;

        if (this.dataSourceService.EnablePaging && !this.dataSourceService.SortItems.length && this.dataSourceService.Entities.length)
            this.dataSourceService.SortItems.push({
                Type: 0,
                EntityAliasName: this.dataSourceService.Entities[0].AliasName,
                Columns:this.dataSourceService.Entities[0].Columns,
                ColumnName: this.dataSourceService.Entities[0].Columns[0]?.ColumnName,
                SortType: 'ASC'
            });
    }

    onSelectedEntityAliasChange(prop) {
        const entity = this.dataSourceService.Entities?.find(e => e.AliasName === prop.EntityAliasName);
        if (!entity) return;

        const matched = this.entities.find(en => en.EntityName === entity.EntityName);
        if (matched) prop.Columns = matched.Columns;
    }

    onAddEntityClick() {
        this.selectedEntity = {};
        this.searchEntities = '';
        this.entities.forEach(e => { e.IsSelected = false; });

        window['wnSelectEntity'].show();
        this.$timeout(() => {
            this.$scope.$broadcast('focusSearchEntity');
        }, 500);
    }

    onEntityItemClick(entity) {
        this.entities.forEach(e => { e.IsSelected = false; });
        entity.IsSelected = !entity.IsSelected;

        this.selectedEntity.Id = entity.Id;
        this.selectedEntity.TableName = entity.TableName;
        this.selectedEntity.EntityName = entity.EntityName;
        this.selectedEntity.Columns = entity.Columns;
        this.$scope.$broadcast('focusEntityAliasName');
    }

    onSelectEntityClick() {
        this.selectedEntityForm.validated = true;
        this.selectedEntityForm.validator(this.selectedEntity);
        if (!this.selectedEntityForm.valid) return;

        const exists = this.dataSourceService.Entities?.some(e => e.AliasName === this.selectedEntity.AliasName);
        if (!exists) {
            this.dataSourceService.Entities = this.dataSourceService.Entities ?? [];
            this.dataSourceService.Entities.push(Global.clone(this.selectedEntity));
            delete this.selectedEntity;
        }

        window['wnSelectEntity'].hide();
    }

    onEntityExpandClick(entity) {
        const matched = this.entities.find(e => e.Id === entity.Id);
        if (matched) entity.Columns = matched.Columns;
    }

    onDeleteEntityClick(entity, $index) {
        this.dataSourceService.Entities.splice($index, 1);
    }

    onAddJoinRelationshipClick(entity) {
        entity.JoinRelationships = entity.JoinRelationships ?? [];
        entity.JoinRelationships.push({});
    }

    onAddJoinRelationshipClick2() {
        this.dataSourceService.JoinRelationships = this.dataSourceService.JoinRelationships ?? [];
        this.dataSourceService.JoinRelationships.push({});
    }

    onJoinRelationshipEntityChange(relationship, type) {
        const aliasName = type === 1 ? relationship.LeftEntityAliasName : relationship.RightEntityAliasName;
        const entity = this.dataSourceService.Entities?.find(e => e.AliasName === aliasName);
        if (!entity) return;

        if (type === 1) relationship.LeftEntityTableName = entity.TableName;
        else if (type === 2) relationship.RightEntityTableName = entity.TableName;
    }

    onAddFilterClick() {
        this.dataSourceService.Filters = this.dataSourceService.Filters ?? [];
        this.dataSourceService.Filters.push({
            Type: 1,
            ConditionGroupName:
                Global.getSuggestedName(
                    this.dataSourceService.Filters.map(f => f.ConditionGroupName),
                    'ConditionGroup'
                )
        });
    }

    onAddSortItemClick() {
        this.dataSourceService.SortItems = this.dataSourceService.SortItems ?? [];
        this.dataSourceService.SortItems.push({ Type: 0, SortType: 'Asc' });
    }

    onSortItemSelectedEntityAliasChange(sortItem) {
        const entity = this.dataSourceService.Entities?.find(e => e.AliasName === sortItem.EntityAliasName);
        if (entity) sortItem.Columns = entity.Columns;
    }

    onEnablePagingChange() {
        this.service.Params = this.service.Params ?? [];
        if (this.dataSourceService.EnablePaging) {
            this._ensureParam('@PageIndex', 'int', 1);
            this._ensureParam('@PageSize', 'int', 2);
        } else {
            this._removeParam('@pageindex');
            this._removeParam('@pagesize');
        }
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            Entities: {
                rule: (value) => {
                    if (value && value.length) return true;
                },
                required: true,
            },
            AppModelId: {
                id: 'drpAppModelId',
                required: true,
            },
            ModelProperties: {
                rule: (value) => {
                    if (value && value.length) return true;
                },
            },
            SortItems: {
                required: true,
            },
            'Settings.StoredProcedurePostfixName': {
                id: 'txtSpPostfix',
                required: true,
            },
        },
            true,
            this.$scope,
            '$.dataSourceService'
        );

        this.selectedEntityForm = this.validationService.init({
            EntityName: {
                required: true,
            },
            AliasName: {
                id: 'txtselectedEntityAliasName',
                rule: (value) => {
                    if (this.sqlConstants.sqlAliasReservedWords.has(value))
                        return 'Alias name is not valid';

                    return true;
                },
                required: true,
            },
            TableName: {
                required: true,
            },
        },
            true,
            this.$scope,
            '$.selectedEntity'
        );

        this.entityJoinRelationshipForm = this.validationService.init({
            JoinType: {
                id: 'drpJoinType',
                required: true,
            },
            RightEntityAliasName: {
                id: 'drpRightEntityAliasName',
                required: true,
            },
            JoinConditions: {
                rule: () => true,
                id: 'txtJoinConditions',
                required: true,
            },
        });
    }

    _ensureParam(paramName, paramType, viewOrder) {
        const exists = this.service.Params.some(p => p.ParamName.toLowerCase() === paramName.toLowerCase());
        if (!exists)
            this.service.Params.push({ ParamName: paramName, ParamType: paramType, ViewOrder: viewOrder });
    }

    _removeParam(paramNameLower) {
        this.service.Params = this.service.Params.filter(p => p.ParamName.toLowerCase() !== paramNameLower);
    }

    _validationJoinRelationship() {
        let isValid = true;
        let existsEntities = [];
        this.dataSourceService.Entities?.forEach(e => {
            e.JoinRelationships?.forEach(r => {
                this.entityJoinRelationshipForm.validated = true;
                this.entityJoinRelationshipForm.validator(r);
                if (!this.entityJoinRelationshipForm.valid ||
                    existsEntities.includes(e.AliasName) ||
                    existsEntities.includes(e.RightEntityAliasName))
                    isValid = false;

                existsEntities = existsEntities.concat([e.AliasName, r.RightEntityAliasName]);
            });
        });

        return isValid;
    }

    _validateService(defer) {
        this.form.validated = true;
        this.form.validator(this.dataSourceService);
        if (this.form.valid && this._validationJoinRelationship()) {
            if (!this.dataSourceService.Settings.StoredProcedureNameModified)
                this.dataSourceService.StoredProcedureName =
                    this.dataSourceService.Settings.StoredProcedurePrefixName +
                    this.dataSourceService.Settings.StoredProcedurePostfixName;

            this.serviceController.extensionService = this.dataSourceService;
        }

        defer.resolve(this.form.valid);
    }

    //#endregion
}

const DataSourceService = {
    bindings: {
        serviceController: '<',
        service: '<'
    },
    controller: DataSourceServiceController,
    controllerAs: '$',
    templateUrl: template,
};

export default DataSourceService;
