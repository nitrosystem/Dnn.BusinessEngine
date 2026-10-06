import template from './delete-entity-row.html';
import { deleteEntityRow_baseQuery } from './sql-query-template.js';
import { Global } from '../../shared/global.js';

class DeleteEntityRowServiceController {
    constructor($scope, $rootScope, validationService) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.validationService = validationService;

        this.baseQueryTemplate = deleteEntityRow_baseQuery;

        $scope.$on('onValidateService', (e, task, args) => {
            this._validateService(task, args);
        });

        const serviceNameWatch = $scope.$watch('$.service.ServiceName', (newVal, oldVal) => {
            if (newVal !== oldVal && !this.deleteEntityRowService.Settings.StoredProcedureNameModified)
                this.deleteEntityRowService.Settings.StoredProcedurePostfixName = newVal;

            if (this.deleteEntityRowService.Settings.StoredProcedureNameModified)
                serviceNameWatch();
        });
    }

    //#region Event Methods

    $onInit() {
        this.onPageLoad();
    }

    onPageLoad() {
        this.entities = this.serviceController.extensionDependency.Entities;
        this.deleteEntityRowService = this.serviceController.extensionService ??
        {
            BaseQuery: this.baseQueryTemplate,
            Conditions: [],
            Settings: {
                StoredProcedurePrefixName: this.$rootScope.scenario.DatabaseObjectPrefix,
                ...(this.service && { StoredProcedurePostfixName: this.service.ServiceName })
            }
        };

        this._setForm();
    }

    onResetBaseQueryClick() {
        this.deleteEntityRowService.BaseQuery = this.baseQueryTemplate;
    }

    onSelectedEntityChange() {
        const entity = this.entities.find(e => e.Id === this.deleteEntityRowService.EntityId);
        if (entity) this.deleteEntityRowService.EntityTableName = entity.TableName;
    }

    onAddFilterClick() {
        this.deleteEntityRowService.Conditions = this.deleteEntityRowService.Conditions ?? [];
        this.deleteEntityRowService.Conditions.push({
            Type: 1,
            ConditionGroupName:
                Global.getSuggestedName(
                    this.deleteEntityRowService.Filters.map(f => f.ConditionGroupName),
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
            '$.deleteEntityRowService'
        );
    }

    _validateService(defer) {
        this.form.validated = true;
        this.form.validator(this.deleteEntityRowService);
        if (this.form.valid) {
            if (!this.deleteEntityRowService.Settings.StoredProcedureNameModified)
                this.deleteEntityRowService.StoredProcedureName =
                    this.deleteEntityRowService.Settings.StoredProcedurePrefixName +
                    this.deleteEntityRowService.Settings.StoredProcedurePostfixName;

            this.serviceController.extensionService = this.deleteEntityRowService;
        }

        defer.resolve(this.form.valid);
    }

    //#endregion
}

const DeleteEntityRowService = {
    bindings: {
        serviceController: '<',
        service: '<'
    },
    controller: DeleteEntityRowServiceController,
    controllerAs: '$',
    templateUrl: template,
};

export default DeleteEntityRowService;
