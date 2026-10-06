import template from './data-row.html';
import { baseQuery } from './sql-query-template.js';
import { Global } from '../../shared/global.js';

class DataRowServiceController {
    constructor($rootScope, $scope, $timeout, validationService, sqlConstants) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.validationService = validationService;
        this.sqlConstants = sqlConstants;

        this.baseQueryTemplate = baseQuery;

        $scope.$on('onValidateService_DataRow', (e, defer) => {
            this._validateService(defer);
        });

        const serviceNameWatch = $scope.$watch('$.service.ServiceName', (newVal, oldVal) => {
            if (newVal !== oldVal && !this.dataRowService.Settings.StoredProcedureNameModified)
                this.dataRowService.Settings.StoredProcedurePostfixName = newVal;

            if (this.dataRowService.Settings.StoredProcedureNameModified)
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
        this.dataRowService = this.serviceController.extensionService ??
        {
            BaseQuery: this.baseQueryTemplate,
            Entities: [],
            Filters: [],
            Settings: {
                StoredProcedurePrefixName: this.$rootScope.scenario.DatabaseObjectPrefix,
                ...(this.service && { StoredProcedurePostfixName: this.service.ServiceName })
            }
        };

        this.dataRowService.Entities?.forEach(e => {
            const matched = this.entities.find(ee => ee.Id === e.Id);
            if (matched) e.Columns = matched.Columns;
        });

        this.dataRowService.ModelProperties?.forEach(prop => this.onSelectedEntityAliasChange(prop));

        this._setForm();
    }

    onResetBaseQueryClick() {
        this.dataRowService.BaseQuery = this.baseQueryTemplate;
    }

    onSelectedAppModelChange() {
        this.onRefreshAppModelClick();
    }

    onRefreshAppModelClick() {
        const result = [];
        const appModel = this.appModels.find(v => v.Id === this.dataRowService.AppModelId);
        if (!appModel) {
            this.dataRowService.ModelProperties = result;
            return;
        }

        appModel.Properties?.forEach(prop => {
            const property = {
                Id: prop.Id,
                PropertyName: prop.PropertyName,
                ValueType: 'DataSource'
            };

            const existing = this.dataRowService.ModelProperties?.find(p => p.Id === prop.Id);
            if (existing) {
                property.IsSelected = existing.IsSelected;
                property.ValueType = existing.ValueType;
                property.EntityAliasName = existing.EntityAliasName;
                property.ColumnName = existing.ColumnName;
                property.Value = existing.Value;
            }

            if (!property.IsSelected && !property.Value) {
                this.dataRowService.Entities?.forEach(entity => {
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

        this.dataRowService.ModelProperties = result;
    }

    onSelectedEntityAliasChange(prop) {
        const entity = this.dataRowService.Entities?.find(e => e.AliasName === prop.EntityAliasName);
        if (!entity) return;

        const matched = this.entities.find(en => en.EntityName === entity.EntityName);
        if (matched) prop.Columns = matched.Columns;
    }

    onAddEntityClick() {
        this.selectedEntity = {};
        this.searchEntities = '';
        this.entities.forEach(e => { e.IsSelected = false; });

        window['wnSelectEntity'].show();
        this.$timeout(() => this.$scope.$broadcast('focusSearchEntity'), 500);
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
        const exists = this.dataRowService.Entities?.some(e => e.AliasName === this.selectedEntity.AliasName);
        if (!exists) {
            this.dataRowService.Entities = this.dataRowService.Entities ?? [];
            this.dataRowService.Entities.push(Global.clone(this.selectedEntity));
            delete this.selectedEntity;
        }
        window['wnSelectEntity'].hide();
    }

    onEntityExpandClick(entity) {
        const matched = this.entities.find(e => e.Id === entity.Id);
        if (matched) entity.Columns = matched.Columns;
    }

    onDeleteEntityClick(entity, $index) {
        this.dataRowService.Entities.splice($index, 1);
    }

    onAddJoinRelationshipClick(entity) {
        entity.JoinRelationships = entity.JoinRelationships ?? [];
        entity.JoinRelationships.push({});
    }

    onAddJoinRelationshipClick2() {
        this.dataRowService.JoinRelationships = this.dataRowService.JoinRelationships ?? [];
        this.dataRowService.JoinRelationships.push({});
    }

    onJoinRelationshipEntityChange(relationship, type) {
        const aliasName = type === 1 ? relationship.LeftEntityAliasName : relationship.RightEntityAliasName;
        const entity = this.dataRowService.Entities?.find(e => e.AliasName === aliasName);
        if (!entity) return;

        if (type === 1) relationship.LeftEntityTableName = entity.TableName;
        else if (type === 2) relationship.RightEntityTableName = entity.TableName;
    }

    onAddFilterClick() {
        this.dataRowService.Filters = this.dataRowService.Filters ?? [];
        this.dataRowService.Filters.push({
            Type: 1,
            ConditionGroupName:
                Global.getSuggestedName(
                    this.dataRowService.Filters.map(f => f.ConditionGroupName),
                    'ConditionGroup'
                )
        });
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
            'Settings.StoredProcedurePostfixName': {
                id: 'txtSpPostfix',
                required: true,
            },
        },
            true,
            this.$scope,
            '$.dataRowService'
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

    _validationJoinRelationship() {
        let isValid = true;
        let existsEntities = [];
        this.dataRowService.Entities?.forEach(e => {
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
        this.form.validator(this.dataRowService);
        if (this.form.valid && this._validationJoinRelationship()) {
            if (!this.dataRowService.Settings.StoredProcedureNameModified)
                this.dataRowService.StoredProcedureName =
                    this.dataRowService.Settings.StoredProcedurePrefixName +
                    this.dataRowService.Settings.StoredProcedurePostfixName;

            this.serviceController.extensionService = this.dataRowService;
        }

        defer.resolve(this.form.valid);
    }

    //#endregion
}

const DataRowService = {
    bindings: {
        serviceController: '<',
        service: '<'
    },
    controller: DataRowServiceController,
    controllerAs: '$',
    templateUrl: template,
};

export default DataRowService;
