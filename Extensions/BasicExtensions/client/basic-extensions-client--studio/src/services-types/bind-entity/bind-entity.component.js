import template from './bind-entity.html';
import { bindEntity_baseQuery } from './sql-query-template.js';
import { Global } from '../../shared/global.js';

class BindEntityServiceController {
    constructor($scope, $rootScope, validationService) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.validationService = validationService;

        this.baseQueryTemplate = bindEntity_baseQuery;

        $scope.$on('onValidateService_BindEntity', (e, defer) => {
            this._validateService(defer);
        });

        const serviceNameWatch = $scope.$watch('$.service.ServiceName', (newVal, oldVal) => {
            if (newVal !== oldVal && !this.bindEntityService.Settings.StoredProcedureNameModified)
                this.bindEntityService.Settings.StoredProcedurePostfixName = newVal;

            if (this.bindEntityService.Settings.StoredProcedureNameModified)
                serviceNameWatch();
        });

        const baseQueryWatch = $scope.$watch('$.bindEntityService.BaseQuery', (newVal, oldVal) => {
            if (newVal !== oldVal && !this.bindEntityService.Settings.StoredProcedureNameModified)
                this.bindEntityService.Settings.BaseQueryModified = newVal;

            if (this.bindEntityService.Settings.BaseQueryModified)
                baseQueryWatch();
        });
    }

    //#region Event Methods

    $onInit() {
        this.onPageLoad();
    }

    onPageLoad() {
        this.entities = this.serviceController.extensionDependency.Entities;
        this.appModels = this.serviceController.extensionDependency.AppModels;
        this.bindEntityService = this.serviceController.extensionService ??
        {
            BaseQuery: this.baseQueryTemplate,
            ModelProperties: [],
            Filters: [],
            Settings: {
                StoredProcedurePrefixName: this.$rootScope.scenario.DatabaseObjectPrefix,
                ...(this.service && { StoredProcedurePostfixName: this.service.ServiceName })
            }
        };

        if (this.bindEntityService.Id) {
            this.bindEntityService.BaseQuery = this.bindEntityService.BaseQuery || this.baseQueryTemplate;
            this.onSelectedEntityChange();
            this.onRefreshAppModelClick();
        }

        this._setForm();
    }

    onResetBaseQueryClick() {
        this.bindEntityService.BaseQuery = this.baseQueryTemplate;
    }

    onSelectedEntityChange() {
        const entity = this.entities.find(e => e.Id === this.bindEntityService.EntityId);
        if (entity) {
            this.bindEntityService.EntityTableName = entity.TableName;
            this.entity = entity;
        }
    }

    onSelectedAppModelChange() {
        this.onRefreshAppModelClick();
    }

    onRefreshAppModelClick() {
        const result = [];
        const appModel = this.appModels.find(v => v.Id === this.bindEntityService.AppModelId);
        if (appModel) {
            appModel.Properties?.forEach(modelProp => {
                const property = {
                    Id: modelProp.Id,
                    PropertyName: modelProp.PropertyName,
                    ViewOrder: modelProp.ViewOrder
                };

                if (!this.bindEntityService.ModelProperties?.length) {
                    property.IsSelected = true;
                    property.ColumnName = modelProp.PropertyName;
                }
                else {
                    const prop = this.bindEntityService.ModelProperties?.find(p => p.Id === modelProp.Id);
                    if (prop) {
                        property.IsSelected = prop.IsSelected;
                        property.ColumnName = prop?.Value ?? modelProp.PropertyName;
                    }
                }

                result.push(property);
            });
        }

        this.bindEntityService.ModelProperties = result;
    }

    onAddFilterClick() {
        this.bindEntityService.Filters = this.bindEntityService.Filters ?? [];
        this.bindEntityService.Filters.push({
            Type: 1,
            ConditionGroupName:
                Global.getSuggestedName(
                    this.bindEntityService.Filters.map(f => f.ConditionGroupName),
                    'ConditionGroup'
                )
        });
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            EntityId: {
                id: 'drpEntityId',
                required: true,
            },
            EntityTableName: {
                required: true,
            },
            'Settings.StoredProcedurePostfixName': {
                id: 'txtSpPostfix',
                required: true
            },
        },
            true,
            this.$scope,
            '$.bindEntityService'
        );
    }

    _validateService(defer) {
        this.form.validated = true;
        this.form.validator(this.bindEntityService);
        if (this.form.valid) {
            if (!this.bindEntityService.Settings.StoredProcedureNameModified)
                this.bindEntityService.StoredProcedureName =
                    this.bindEntityService.Settings.StoredProcedurePrefixName +
                    this.bindEntityService.Settings.StoredProcedurePostfixName;

            this.serviceController.extensionService = this.bindEntityService;
        }

        defer.resolve(this.form.valid);
    }

    //#endregion

}

const BindEntityService = {
    bindings: {
        serviceController: '<',
        service: '<'
    },
    controller: BindEntityServiceController,
    controllerAs: '$',
    templateUrl: template,
};

export default BindEntityService;