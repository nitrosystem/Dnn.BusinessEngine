import template from "./custom-query.html";

class CustomQueryServiceController {
    constructor(
        $scope,
        $rootScope,
        $q,
        $timeout,
        $deferredEmit,
        globalService,
        apiService,
        validationService
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$q = $q;
        this.$timeout = $timeout;
        this.$deferredEmit = $deferredEmit;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;

        $scope.$on("onValidateService", (e, task, args) => {
            this.validateService.apply(this, [task, args]);
        });

        $scope.$watch('$.service.ServiceName', (newVal, oldVal) => {
            if (newVal != oldVal && !this.customQueryService.Settings.StoredProcedureNameModified)
                this.customQueryService.Settings.StoredProcedurePostfixName = newVal;
        });

        this.setForm();
    }

    $onInit() {
        this.onPageLoad();
    }

    onPageLoad() {
        this.customQueryService = this.serviceController.extensionService ??
        {
            Query: "CREATE PROCEDURE {Schema}.{ProcedureName}\nAS BEGIN\n\t\nEND",
            Settings: {
                StoredProcedurePrefixName: this.$rootScope.scenario.DatabaseObjectPrefix,
                ...(this.service && { StoredProcedurePostfixName: this.service.ServiceName })
            }
        };
    }

    setForm() {
        this.form = this.validationService.init({
            Query: {
                id: "editorSqlQuery",
                required: true,
            },
            "Settings.StoredProcedurePostfixName": {
                id: "txtSpPostfix",
                required: true,
            },
        },
            true,
            this.$scope,
            "$.customQueryService"
        );
    }

    onDetectServiceParams() {
        this.service.Params = this.globalService.getParamsFromSqlQuery(
            this.customQueryService.Query
        );
    }

    validateService(task, args) {
        task.wait(() => {
            var defer = this.$q.defer();

            this.form.validated = true;
            this.form.validator(this.customQueryService);
            if (this.form.valid) {
                this.customQueryService.StoredProcedureName =
                    this.customQueryService.Settings.StoredProcedurePrefixName +
                    this.customQueryService.Settings.StoredProcedurePostfixName;

                this.serviceController.extensionService = this.customQueryService;

                defer.resolve(true);
            }

            return defer.promise;
        });
    }
}

const CustomQueryService = {
    bindings: {
        serviceController: "<",
        service: "<"
    },
    controller: CustomQueryServiceController,
    controllerAs: "$",
    templateUrl: template,
};

export default CustomQueryService;