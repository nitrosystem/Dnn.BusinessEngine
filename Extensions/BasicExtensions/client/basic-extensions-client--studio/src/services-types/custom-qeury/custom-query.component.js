import template from './custom-query.html';

class CustomQueryServiceController {
    constructor(
        $rootScope,
        $scope,
        validationService
    ) {
        'ngInject';

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.validationService = validationService;

        $scope.$on('onValidateService_CustomQuery', (e, defer) => {
            this._validateService(defer);
        });

        const serviceNameWatch = $scope.$watch('$.service.ServiceName', (newVal, oldVal) => {
            if (newVal !== oldVal && !this.customQueryService.Settings.StoredProcedureNameModified)
                this.customQueryService.Settings.StoredProcedurePostfixName = newVal;

            if (this.customQueryService.Settings.StoredProcedureNameModified)
                serviceNameWatch();
        });
    }

    //#region Event Methods

    $onInit() {
        this.onPageLoad();
    }

    onPageLoad() {
        this.appModels = this.serviceController.extensionDependency.AppModels;
        this.customQueryService = this.serviceController.extensionService ??
        {
            Query: 'CREATE PROCEDURE {Schema}.{ProcedureName}\nAS BEGIN\n\t\nEND',
            Settings: {
                StoredProcedurePrefixName: this.$rootScope.scenario.DatabaseObjectPrefix,
                ...(this.service && { StoredProcedurePostfixName: this.service.ServiceName })
            }
        };

        this._setForm();
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            Query: {
                id: 'editorSqlQuery',
                required: true,
            },
            'Settings.StoredProcedurePostfixName': {
                id: 'txtSpPostfix',
                required: true,
            },
        },
            true,
            this.$scope,
            '$.customQueryService'
        );
    }

    _validateService(defer) {
        this.form.validated = true;
        this.form.validator(this.customQueryService);
        if (this.form.valid) {
            this.customQueryService.StoredProcedureName =
                this.customQueryService.Settings.StoredProcedurePrefixName +
                this.customQueryService.Settings.StoredProcedurePostfixName;

            this.serviceController.extensionService = this.customQueryService;
        }

        defer.resolve(this.form.valid);
    }

    //#endregion
}

const CustomQueryService = {
    bindings: {
        serviceController: '<',
        service: '<'
    },
    controller: CustomQueryServiceController,
    controllerAs: '$',
    templateUrl: template,
};

export default CustomQueryService;