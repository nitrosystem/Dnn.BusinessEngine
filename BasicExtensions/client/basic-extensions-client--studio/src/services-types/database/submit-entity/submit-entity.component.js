import template from "./submit-entity.html";
import { baseQuery_insert, baseQuery_insertupdate, baseQuery_update } from './sql-query-template.js';

class SubmitEntityServiceController {
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

        this.baseQueryInsertTemplate = baseQuery_insert;
        this.baseQueryInsertUpdateTemplate = baseQuery_insertupdate;
        this.baseQueryUpdateTemplate = baseQuery_update;

        $scope.$on("onValidateService", (e, task, args) => {
            this.validateService.apply(this, [task, args]);
        });

        $scope.$watch('$.service.ServiceName', (newVal, oldVal) => {
            if (newVal != oldVal && !this.submitEntityService.Settings.StoredProcedureNameModified)
                this.submitEntityService.Settings.StoredProcedurePostfixName = newVal;
        });

        this.setForm();
    }

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

        if (this.submitEntityService.EntityId) {
            _.filter(this.entities, (e) => {
                return this.submitEntityService.EntityId == e.Id;
            }).map((ee) => {
                ee.Columns = ee.Columns;
            });
        }

        this.setForm();
    }

    setForm() {
        this.form = this.validationService.init({
            ActionType: {
                required: true,
            },
            EntityId: {
                id: "drpEntityId",
                required: true,
            },
            Entity: {
                required: true,
            },
            "Entity.InsertColumns": {
                rule: (value) => {
                    if (
                        (this.submitEntityService.ActionType == 0 ||
                            this.submitEntityService.ActionType == 1) &&
                        _.filter(value || [], (c) => {
                            return c.IsSelected;
                        }).length == 0
                    )
                        return "Select column(s) for insert query.";
                    else return true;
                },
            },
            "Entity.UpdateColumns": {
                rule: (value) => {
                    if (
                        (this.submitEntityService.ActionType == 0 ||
                            this.submitEntityService.ActionType == 2) &&
                        _.filter(value || [], (c) => {
                            return c.IsSelected;
                        }).length == 0
                    )
                        return "Select column(s) for update query.";
                    else return true;
                },
            },
            "Settings.StoredProcedurePostfixName": {
                id: "txtSpPostfix",
                required: true,
            },
        },
            true,
            this.$scope,
            "$.submitEntityService"
        );
    }

    onResetBaseQueryClick() {
        this.submitEntityService.BaseQuery = '';
        this.onActionTypeChange();
    }

    onSelectedEntityChange() {
        _.filter(this.entities, (e) => {
            return e.Id == this.submitEntityService.EntityId;
        }).map((e) => {
            this.submitEntityService.Entity = e;

            this.submitEntityService.Entity.InsertColumns = [];
            this.submitEntityService.Entity.UpdateColumns = [];

            this.service.Params = this.service.Params || [];

            _.forEach(e.Columns, (c) => {
                var paramName = "@" + c.ColumnName;
                if (
                    _.filter(this.service.Params, (p) => {
                        return p.ParamName == paramName;
                    }).length == 0
                ) {
                    this.service.Params.push({
                        ParamName: paramName,
                        ParamType: c.ColumnType,
                    });
                }
            });

            _.filter(e.Columns, (c) => {
                return c.IsPrimary;
            }).map((c) => {
                this.submitEntityService.Entity.PrimaryKeyParam = "@" + c.ColumnName;

                this.submitEntityService.Entity.InsertConditions = [{
                    SqlQuery: "NOT EXISTS(\n\t\tSELECT [" +
                        c.ColumnName +
                        "] FROM {Schema}.{TableName} \n\t\tWHERE [" +
                        c.ColumnName +
                        "] = @" +
                        c.ColumnName +
                        "\n\t)",
                    GroupName: "Group1",
                },];

                this.submitEntityService.Entity.UpdateConditions = [{
                    SqlQuery: "[" + c.ColumnName + "] = @" + c.ColumnName,
                    GroupName: "Group1",
                },];
            });

            _.forEach(e.Columns, (c) => {
                if (!c.IsIdentity) {
                    this.submitEntityService.Entity.InsertColumns.push(angular.copy(c));
                    this.submitEntityService.Entity.UpdateColumns.push(angular.copy(c));
                }
            });
        });
    }

    onUpdateEntityColumnsClick() {
        _.filter(this.entities, (e) => { return e.Id == this.submitEntityService.EntityId; }).map((e) => {
            if (this.submitEntityService.ActionType == 0 || this.submitEntityService.ActionType == 1)
                this.submitEntityService.Entity.InsertColumns =
                    this.updateColumns(_.filter(e.Columns, (c) => { return !c.IsIdentity; }), this.submitEntityService.Entity.InsertColumns ?? [])
            if (this.submitEntityService.ActionType == 0 || this.submitEntityService.ActionType == 2)
                this.submitEntityService.Entity.UpdateColumns =
                    this.updateColumns(_.filter(e.Columns, (c) => { return !c.IsIdentity; }), this.submitEntityService.Entity.UpdateColumns ?? [])
        });
    }

    updateColumns(sourceColumns, destCoulumns) {
        const sourceColumnIds = _.keyBy(sourceColumns, 'Id');
        const destCoulumnsIds = _.keyBy(destCoulumns, 'Id');

        var updatedColumns =
            destCoulumns.map(col => {
                const updated = sourceColumnIds[col.Id];
                return updated ? { ...col, ...updated } : null;
            }).filter(Boolean);

        var newColumns =
            sourceColumns.map(col => {
                const existed = destCoulumnsIds[col.Id];
                return existed ? null : col;
            }).filter(Boolean);

        return [...updatedColumns, ...newColumns];
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

    onSelectedInsertEntityAllColumnsChange() {
        _.forEach(this.submitEntityService.Entity.InsertColumns, (c) => {
            c.IsSelected = !c.IsSelected;

            this.onSelectedInsertEntityColumnChange(c);
        });
    }

    onSelectedInsertEntityColumnChange(c) {
        var paramName = "@" + c.ColumnName;

        if (
            c.IsSelected &&
            _.filter(this.service.Params, (p) => {
                return p.ParamName == paramName;
            }).length
        ) {
            c.ColumnValue = paramName;
        }
    }

    onSelectedUpdateEntityAllColumnsChange() {
        _.forEach(this.submitEntityService.Entity.UpdateColumns, (c) => {
            c.IsSelected = !c.IsSelected;

            this.onSelectedUpdateEntityColumnChange(c);
        });
    }

    onSelectedUpdateEntityColumnChange(c) {
        var paramName = "@" + c.ColumnName;

        if (
            c.IsSelected &&
            _.filter(this.service.Params, (p) => {
                return p.ParamName == paramName;
            }).length
        ) {
            c.ColumnValue = paramName;
        }
    }

    validateService(task, args) {
        task.wait(() => {
            var defer = this.$q.defer();

            this.form.validated = true;
            this.form.validator(this.submitEntityService);
            if (this.form.valid) {
                this.submitEntityService.StoredProcedureName =
                    this.submitEntityService.Settings.StoredProcedurePrefixName +
                    this.submitEntityService.Settings.StoredProcedurePostfixName;

                this.serviceController.extensionService = this.submitEntityService;

                defer.resolve(true);
            }

            return defer.promise;
        });
    }
}

const SubmitEntityService = {
    bindings: {
        serviceController: "<",
        service: "<"
    },
    controller: SubmitEntityServiceController,
    controllerAs: "$",
    templateUrl: template,
};

export default SubmitEntityService;