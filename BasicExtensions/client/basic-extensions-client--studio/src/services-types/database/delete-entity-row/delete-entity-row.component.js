import template from "./delete-entity-row.html";
import { deleteEntityRow_baseQuery } from './sql-query-template.js';

class DeleteEntityRowServiceController {
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

        this.baseQueryTemplate = deleteEntityRow_baseQuery;

        $scope.$on("onValidateService", (e, task, args) => {
            this.validateService.apply(this, [task, args]);
        });

        $scope.$watch('$.service.ServiceName', (newVal, oldVal) => {
            if (newVal != oldVal && !this.deleteEntityRowService.Settings.StoredProcedureNameModified)
                this.deleteEntityRowService.Settings.StoredProcedurePostfixName = newVal;
        });

        this.setForm();
    }

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
    }

    setForm() {
        this.form = this.validationService.init({
            EntityId: {
                id: "drpEntityId",
                required: true,
            },
            EntityTableName: {
                required: true,
            },
            "Settings.StoredProcedurePostfixName": {
                id: "txtSpPostfix",
                required: true
            },
        },
            true,
            this.$scope,
            "$.deleteEntityRowService"
        );
    }

    onResetBaseQueryClick() {
        this.deleteEntityRowService.BaseQuery = this.baseQueryTemplate;
    }

    onSelectedEntityChange() {
        _.filter(this.entities, (e) => { return e.Id == this.deleteEntityRowService.EntityId }).map((e) => {
            this.deleteEntityRowService.EntityTableName = e.TableName;
        })
    }

    onAddFilterClick() {
        this.deleteEntityRowService.Conditions = this.deleteEntityRowService.Conditions || [];
        this.deleteEntityRowService.Conditions.push({
            Type: 1,
            ConditionGroupName: "ConditionGroup" + (this.deleteEntityRowService.Conditions.length + 1),
        });
    }

    validateService(task, args) {
        task.wait(() => {
            var defer = this.$q.defer();

            this.form.validated = true;
            this.form.validator(this.deleteEntityRowService);
            if (this.form.valid) {
                this.deleteEntityRowService.StoredProcedureName =
                    this.deleteEntityRowService.Settings.StoredProcedurePrefixName +
                    this.deleteEntityRowService.Settings.StoredProcedurePostfixName;

                this.serviceController.extensionService = this.deleteEntityRowService;

                defer.resolve(true);
            }

            return defer.promise;
        });
    }
}

const DeleteEntityRowService = {
    bindings: {
        service: "<"
    },
    controller: DeleteEntityRowServiceController,
    controllerAs: "$",
    templateUrl: template,
};

export default DeleteEntityRowService;