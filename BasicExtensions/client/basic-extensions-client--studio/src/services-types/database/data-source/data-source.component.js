import template from "./data-source.html";
import { baseQuery } from './sql-query-template.js';

class DataSourceServiceController {
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
            if (newVal != oldVal && !this.dataSourceService.Settings.StoredProcedureNameModified)
                this.dataSourceService.Settings.StoredProcedurePostfixName = newVal;
        });

        this.setForm();
    }

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
            }
        };

        if (!this.service.Id) {
            if (this.service.ResultType == 2) {
                this.service.Params = [
                    { ParamName: "@PageIndex", ParamType: "int", DefaultValue: 1 },
                    { ParamName: "@PageSize", ParamType: "int", DefaultValue: 10 },
                ];

                this.dataSourceService.EnablePaging = true;
                this.dataSourceService.PageIndexParam = "@PageIndex";
                this.dataSourceService.PageSizeParam = "@PageSize";
            }
        }

        (this.dataSourceService.Entities || []).forEach((e) => {
            _.filter(this.entities, (ee) => {
                return ee.Id == e.Id;
            }).map((ee) => {
                e.Columns = ee.Columns;
            });
        });

        (this.dataSourceService.AppModelProperties || []).forEach((prop) => {
            this.onSelectedEntityAliasChange(prop);
        });

        (this.dataSourceService.SortItems || []).forEach((s) => {
            this.onSortItemSelectedEntityAliasChange(s);
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
            AppModelProperties: {
                rule: (value) => {
                    if (value && value.length) {
                        return true;
                    }
                },
            },
            // SortItems: {
            //   rule: (value) => {
            //     if (this.service.ResultType == 2 && value && value.length) {
            //       return true;
            //     }

            //     return true;
            //   },
            // },
            "Settings.StoredProcedurePostfixName": {
                id: "txtSpPostfix",
                required: true,
            },
        },
            true,
            this.$scope,
            "$.dataSourceService"
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
        this.dataSourceService.BaseQuery = this.baseQueryTemplate;
    }

    onSelectedAppModelChange() {
        this.onRefreshAppModelClick();
    }

    onRefreshAppModelClick() {
        var result = [];

        const appModel = _.find(this.appModels, (v) => {
            return v.Id == this.dataSourceService.AppModelId;
        });

        _.forEach(appModel.Properties, (prop) => {
            var property = {
                Id: prop.Id,
                PropertyName: prop.PropertyName,
                ValueType: "DataSource",
            };


            _.filter(this.dataSourceService.AppModelProperties, (p) => {
                return p.Id == prop.Id;
            }).map((p) => {
                property.IsSelected = p.IsSelected;
                property.ValueType = p.ValueType;
                property.EntityAliasName = p.EntityAliasName;
                property.ColumnName = p.ColumnName;
                property.Value = p.Value;
            });

            if (!property.IsSelected && !property.Value) {
                _.forEach(this.dataSourceService.Entities, (entity) => {
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

        this.dataSourceService.AppModelProperties = result;
    }

    onSelectedEntityAliasChange(prop) {
        _.filter(this.dataSourceService.Entities, (e) => {
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
                _.filter(this.dataSourceService.Entities, (e) => {
                    return e.AliasName == this.selectedEntity.AliasName;
                }).length == 0
            ) {
                this.dataSourceService.Entities = this.dataSourceService.Entities || [];
                this.dataSourceService.Entities.push(angular.copy(this.selectedEntity));

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
        this.dataSourceService.Entities.splice($index, 1);
    }

    onAddJoinRelationshipClick(entity) {
        entity.JoinRelationships = entity.JoinRelationships || [];
        entity.JoinRelationships.push({});
    }

    onAddJoinRelationshipClick2() {
        this.dataSourceService.JoinRelationships =
            this.dataSourceService.JoinRelationships || [];
        this.dataSourceService.JoinRelationships.push({});
    }

    onJoinRelationshipEntityChange(relationship, type) {
        _.filter(this.dataSourceService.Entities, (e) => {
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
        this.dataSourceService.Filters = this.dataSourceService.Filters || [];
        this.dataSourceService.Filters.push({
            Type: 1,
            ConditionGroupName: "ConditionGroup" + (this.dataSourceService.Filters.length + 1),
        });
    }

    onAddSortItemClick() {
        this.dataSourceService.SortItems = this.dataSourceService.SortItems || [];
        this.dataSourceService.SortItems.push({ Type: 0, SortType: "Asc" });
    }

    onSortItemSelectedEntityAliasChange(sortItem) {
        _.filter(this.dataSourceService.Entities, (e) => {
            return e.AliasName == sortItem.EntityAliasName;
        }).map((e) => {
            sortItem.Columns = e.Columns;
        });
    }

    onEnablePagingChange() {
        if (this.dataSourceService.EnablePaging) {
            if (
                _.filter(this.service.Params, (p) => {
                    return p.ParamName.toLowerCase() == "@pageindex";
                }).length == 0
            )
                this.service.Params.push({
                    ParamName: "@PageIndex",
                    ParamType: "int",
                    DefaultValue: 1,
                    IsSystemParam: true,
                });

            if (
                _.filter(this.service.Params, (p) => {
                    return p.ParamName.toLowerCase() == "@pagesize";
                }).length == 0
            )
                this.service.Params.push({
                    ParamName: "@PageSize",
                    ParamType: "int",
                    DefaultValue: 1,
                    IsSystemParam: true,
                });
        } else {
            _.filter(this.service.Params, (p) => {
                return p.ParamName.toLowerCase() == "@pageindex";
            }).map((p) => {
                this.service.Params.splice(this.service.Params.indexOf(p), 1);
            });

            _.filter(this.service.Params, (p) => {
                return p.ParamName.toLowerCase() == "@pagesize";
            }).map((p) => {
                this.service.Params.splice(this.service.Params.indexOf(p), 1);
            });
        }
    }

    validationJoinRelationship() {
        var isValid = true;

        var existsEntities = [];

        (this.dataSourceService.Entities || []).forEach((e) => {
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
            this.form.validator(this.dataSourceService);
            if (this.form.valid && this.validationJoinRelationship()) {
                this.dataSourceService.StoredProcedureName =
                    this.dataSourceService.Settings.StoredProcedurePrefixName +
                    this.dataSourceService.Settings.StoredProcedurePostfixName;

                this.serviceController.extensionService = this.dataSourceService;

                defer.resolve(true);
            }

            return defer.promise;
        });
    }
}

const DataSourceService = {
    bindings: {
        serviceController: "<",
        service: "<",
    },
    controller: DataSourceServiceController,
    controllerAs: "$",
    templateUrl: template,
};

export default DataSourceService;