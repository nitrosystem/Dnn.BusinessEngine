import template from "./bind-entity.html";
import { bindEntity_baseQuery } from './sql-query-template.js';

class BindEntityServiceController {
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

        this.baseQueryTemplate = bindEntity_baseQuery;

        $scope.$on("onValidateService", (e, task, args) => {
            this.validateService.apply(this, [task, args]);
        });

        $scope.$watch('$.service.ServiceName', (newVal, oldVal) => {
            if (newVal != oldVal && !this.bindEntityService.Settings.StoredProcedureNameModified)
                this.bindEntityService.Settings.StoredProcedurePostfixName = newVal;
        });

        this.setForm();
    }

    $onInit() {
        this.onPageLoad();
    }

    onPageLoad() {
        this.entities = this.serviceController.extensionDependency.Entities;
        this.bindEntityService = this.serviceController.extensionService ??
        {
            BaseQuery: this.baseQueryTemplate,
            Filters: [],
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
            "$.bindEntityService"
        );
    }

    onResetBaseQueryClick() {
        this.bindEntityService.BaseQuery = this.baseQueryTemplate;
    }

    onSelectedEntityChange() {
        _.filter(this.entities, (e) => { return e.Id == this.bindEntityService.EntityId }).map((e) => {
            this.bindEntityService.EntityTableName = e.TableName;
        })
    }

    onAddFilterClick() {
        this.bindEntityService.Filters = this.bindEntityService.Filters || [];
        this.bindEntityService.Filters.push({
            Type: 1,
            ConditionGroupName: "ConditionGroup" + (this.bindEntityService.Filters.length + 1),
        });
    }

    validateService(task, args) {
        task.wait(() => {
            var defer = this.$q.defer();

            this.form.validated = true;
            this.form.validator(this.bindEntityService);
            if (this.form.valid) {
                if (!this.bindEntityService.Settings.StoredProcedureNameModified)
                    this.bindEntityService.StoredProcedureName =
                        this.bindEntityService.Settings.StoredProcedurePrefixName +
                        this.bindEntityService.Settings.StoredProcedurePostfixName;

                this.serviceController.extensionService = this.bindEntityService;

                defer.resolve(true);
            }

            return defer.promise;
        });
    }
}

const BindEntityService = {
    bindings: {
        serviceController: "<",
        service: "<"
    },
    controller: BindEntityServiceController,
    controllerAs: "$",
    templateUrl: template,
};

export default BindEntityService;