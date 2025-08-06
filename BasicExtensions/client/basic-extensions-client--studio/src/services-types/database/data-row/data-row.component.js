import template from "./data-row.html";
import { baseQuery } from './sql-query-template.js';

class DataRowServiceController {
    constructor(
        $scope,
        $rootScope,
        $q,
        $timeout,
        $deferredEmit,
        globalService,
        validationService
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$q = $q;
        this.$timeout = $timeout;
        this.$deferredEmit = $deferredEmit;
        this.globalService = globalService;
        this.validationService = validationService;

        this.baseQueryTemplate = baseQuery;

        $scope.$on("onValidateService", (e, task, args) => {
            this.validateService.apply(this, [task, args]);
        });

        $scope.$watch('$.service.ServiceName', (newVal, oldVal) => {
            if (newVal != oldVal && !this.dataRowService.Settings.StoredProcedureNameModified)
                this.dataRowService.Settings.StoredProcedurePostfixName = newVal;
        });

        this.setForm();
    }

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

        (this.dataRowService.Entities || []).forEach((e) => {
            _.filter(this.entities, (ee) => {
                return ee.Id == e.Id;
            }).map((ee) => {
                e.Columns = ee.Columns;
            });
        });

        (this.dataRowService.ModelProperties || []).forEach((prop) => {
            this.onSelectedEntityAliasChange(prop);
        });

    }

    setForm() {
        this.form = this.validationService.init({
            Entities: {
                rule: (value) => {
                    if (value && value.length) {
                        return true;
                    }
                },
                required: true,
            },
            AppModelId: {
                id: "drpAppModelId",
                required: true,
            },
            ModelProperties: {
                rule: (value) => {
                    if (value && value.length) {
                        return true;
                    }
                },
            },
            "Settings.StoredProcedurePostfixName": {
                id: "txtSpPostfix",
                required: true,
            },
        },
            true,
            this.$scope,
            "$.dataRowService"
        );

        this.selectedEntityForm = this.validationService.init({
            EntityName: {
                required: true,
            },
            AliasName: {
                id: "txtselectedEntityAliasName",
                required: true,
            },
            TableName: {
                required: true,
            },
        },
            true,
            this.$scope,
            "$.selectedEntity"
        );

        this.entityJoinRelationshipForm = this.validationService.init({
            JoinType: {
                id: "drpJoinType",
                required: true,
            },
            RightEntityAliasName: {
                id: "drpRightEntityAliasName",
                required: true,
            },
            JoinConditions: {
                rule: (value) => {
                    return true;
                },
                id: "txtJoinConditions",
                required: true,
            },
        });
    }

    onResetBaseQueryClick() {
        this.dataRowService.BaseQuery = this.baseQueryTemplate;
    }

    onSelectedAppModelChange() {
        this.onRefreshAppModelClick();
    }

    onRefreshAppModelClick() {
        var result = [];

        const appModel = _.find(this.appModels, (v) => {
            return v.Id == this.dataRowService.AppModelId;
        });

        _.forEach(appModel.Properties, (prop) => {
            var property = {
                Id: prop.Id,
                PropertyName: prop.PropertyName,
                ValueType: "DataSource",
            };


            _.filter(this.dataRowService.ModelProperties, (p) => {
                return p.Id == prop.Id;
            }).map((p) => {
                property.IsSelected = p.IsSelected;
                property.ValueType = p.ValueType;
                property.EntityAliasName = p.EntityAliasName;
                property.ColumnName = p.ColumnName;
                property.Value = p.Value;
            });

            if (!property.IsSelected && !property.Value) {
                _.forEach(this.dataRowService.Entities, (entity) => {
                    _.filter(entity.Columns ?? [], (c) => { return c.ColumnName == property.PropertyName }).map((column) => {
                        property.IsSelected = true;
                        property.ValueType = 'DataSource';
                        property.EntityAliasName = entity.AliasName
                        property.Columns = entity.Columns;
                        property.ColumnName = column.ColumnName;
                    });
                });
            }

            result.push(property);
        });

        this.dataRowService.ModelProperties = result;
    }

    onSelectedEntityAliasChange(prop) {
        _.filter(this.dataRowService.Entities, (e) => {
            return e.AliasName == prop.EntityAliasName;
        }).map((e) => {
            _.filter(this.entities, (en) => { return en.EntityName == e.EntityName; }).map((en) => {
                prop.Columns = en.Columns;
            })
        });
    }

    onAddEntityClick() {
        this.selectedEntity = {};
        this.searchEntities = "";
        this.entities.forEach((e) => {
            e.IsSelected = false;
        });

        window["wnSelectEntity"].show();

        this.$timeout(() => {
            this.$scope.$broadcast("focusSearchEntity");
        }, 500);
    }

    onEntityItemClick(entity) {
        this.entities.forEach((e) => {
            e.IsSelected = false;
        });

        entity.IsSelected = !entity.IsSelected;

        this.selectedEntity.Id = entity.Id;
        this.selectedEntity.TableName = entity.TableName;
        this.selectedEntity.EntityName = entity.EntityName;
        this.selectedEntity.Columns = entity.Columns;

        this.$scope.$broadcast("focusEntityAliasName");
    }

    onSelectEntityClick() {
        this.selectedEntityForm.validated = true;
        this.selectedEntityForm.validator(this.selectedEntity);
        if (this.selectedEntityForm.valid) {
            if (
                _.filter(this.dataRowService.Entities, (e) => {
                    return e.AliasName == this.selectedEntity.AliasName;
                }).length == 0
            ) {
                this.dataRowService.Entities = this.dataRowService.Entities || [];
                this.dataRowService.Entities.push(angular.copy(this.selectedEntity));

                delete this.selectedEntity;
            }

            window["wnSelectEntity"].hide();
        }
    }

    onEntityExpandClick(entity) {
        _.filter(this.entities, (e) => {
            return e.Id == entity.Id;
        }).map((e) => {
            entity.Columns = e.Columns;
        });
    }

    onDeleteEntityClick(entity, $index) {
        this.dataRowService.Entities.splice($index, 1);
    }

    onAddJoinRelationshipClick(entity) {
        entity.JoinRelationships = entity.JoinRelationships || [];
        entity.JoinRelationships.push({});
    }

    onAddJoinRelationshipClick2() {
        this.dataRowService.JoinRelationships =
            this.dataRowService.JoinRelationships || [];
        this.dataRowService.JoinRelationships.push({});
    }

    onJoinRelationshipEntityChange(relationship, type) {
        _.filter(this.dataRowService.Entities, (e) => {
            return (
                (type == 1 && e.AliasName == relationship.LeftEntityAliasName) ||
                (type == 2 && e.AliasName == relationship.RightEntityAliasName)
            );
        }).map((e) => {
            if (type == 1) relationship.LeftEntityTableName = e.TableName;
            else if (type == 2) relationship.RightEntityTableName = e.TableName;
        });
    }

    onAddFilterClick() {
        this.dataRowService.Filters = this.dataRowService.Filters || [];
        this.dataRowService.Filters.push({
            Type: 1,
            ConditionGroupName: "ConditionGroup" + (this.dataRowService.Filters.length + 1),
        });
    }

    validationJoinRelationship() {
        var isValid = true;

        var existsEntities = [];

        (this.dataRowService.Entities || []).forEach((e) => {
            e.JoinRelationships || [].forEach((r) => {
                this.entityJoinRelationshipForm.validated = true;
                this.entityJoinRelationshipForm.validator(r);
                if (!this.entityJoinRelationshipForm.valid ||
                    existsEntities.indexOf(e.AliasName) >= 0 ||
                    existsEntities.indexOf(e.RightEntityAliasName) >= 0
                )
                    isValid = false;

                existsEntities = existsEntities.concat([
                    e.AliasName,
                    r.RightEntityAliasName,
                ]);
            });
        });

        return isValid;
    }

    validateService(task, args) {
        task.wait(() => {
            var defer = this.$q.defer();

            this.form.validated = true;
            this.form.validator(this.dataRowService);
            if (this.form.valid && this.validationJoinRelationship()) {
                this.dataRowService.StoredProcedureName =
                    this.dataRowService.Settings.StoredProcedurePrefixName +
                    this.dataRowService.Settings.StoredProcedurePostfixName;

                this.serviceController.extensionService = this.dataRowService;

                defer.resolve(true);
            }

            return defer.promise;
        });
    }
}

const DataRowService = {
    bindings: {
        serviceController: "<",
        service: "<",
    },
    controller: DataRowServiceController,
    controllerAs: "$",
    templateUrl: template,
};

export default DataRowService;