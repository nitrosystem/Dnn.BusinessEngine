import { GlobalUtil } from '../../utils';
import template from './select-service.component.html';

class SelectServiceController {
    constructor($scope, $timeout, $q, apiService, notificationService, baseOptions) {
        'ngInject';
        this.$scope = $scope;
        this.$timeout = $timeout;
        this.$q = $q;
        this.baseOptions = baseOptions;

        this.apiService = apiService;
        this.notifyService = notificationService;

        // Reset editing state when Params reference changes
        $scope.$watch('$.action.Params', (newVal, oldVal) => {
            if (this.param && oldVal && newVal !== oldVal) {
                this.param = null;
            }

            newVal?.forEach(c => c.isEdited = null);
        });
    }

    $onInit() {
        this.action = this.action ?? {};
    }

    /**
     * Load services with pagination and optional search.
     * @param {Number} [pageIndex=1]
     * @param {String} [searchText]
     * @returns {Promise}
    */
    getServices(pageIndex, searchText) {
        const defer = this.$q.defer();

        this.running = 'get-services';
        this.awaitAction = {
            title: 'Get Services By Page',
            subtitle: 'Just a moment for get services...',
        };

        this.apiService.get('Studio', 'GetServices', {
            scenarioId: this.baseOptions.scenarioId,
            pageIndex: pageIndex ?? 1,
            pageSize: 10,
            searchText,
            serviceType: this.serviceType,
        }).then((data) => {
            this.services = data.Services;
            this._populateServiceParams();

            defer.resolve(data);

            this.awaitAction = null;
            this.running = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data?.Message);

            defer.reject(error);

            this.running = null;
        });

        return defer.promise;
    }

    onServiceChange() {
        this._populateServiceParams();
    }

    /**
     * Refresh the parameters of the currently selected service.
    */
    onRefreshServiceParamsClick() {
        this.controller.running = 'get-service-params';
        this.controller.awaitAction = {
            title: 'Loading Service Params',
            subtitle: 'Just a moment for loading service params...',
        };

        this.apiService.get('Studio', 'GetServiceParams', { serviceId: this.action.ServiceId }).then((data) => {
            const service = this.services?.find(s => s.Id === this.action.ServiceId);
            if (service) service.Params = data;

            this._populateServiceParams();

            this.controller.running = null;
            this.controller.awaitAction = null;
        });
    }

    onAddServiceClick() {
        this.$scope.$emit('onGotoPage', { page: 'create-service' });
    }

    onEditServiceClick() {
        this.$scope.$emit('onGotoPage', {
            page: 'create-service',
            id: this.action.ServiceId,
        });
    }

    /**
     * Handle row click to enter edit mode.
    */
    onRowItemClick(param, $index) {
        // Commit currently edited param (if any) before switching
        if (this.param) {
            this.param.isEdited = false;

            const index = this.action.Params.findIndex(p => p.ParamName === this.param.ParamName);
            if (index >= 0) {
                this.action.Params[index] = { ...this.param };
            }

            this.param = null;
        }

        this.param = param;
        this.param.isEdited = true;
        this.param.orderId = $index + 1;
        this.$timeout(() => this.$scope.$broadcast('onEditParam'), 500);
    }

    /**
     * Commit the currently edited param.
    */
    onDoneClick() {
        if (!this.param) return;

        this.param.isEdited = false;

        const index = this.action.Params.findIndex(p => p.ParamName === this.param.ParamName);
        if (index >= 0) {
            this.action.Params[index] = { ...this.param };
        }

        this.param = null;
    }

    /**
     * Bind params of the selected service into the action.
    */
    _populateServiceParams() {
        this.action.Params = this.action.Params ?? [];
        const service = this.services?.find(s => s.Id === this.action.ServiceId);
        if (service) GlobalUtil.bindParams(this.action.Params, service.Params);

        this.action.Params.forEach((p) => p.isEdited = false);
    }
}

const SelectServiceComponent = {
    bindings: {
        controller: '<',
        action: '<',
        services: '<',
        serviceType: '@',
        searchText: '@',
        suggestions: '<',
    },
    controller: SelectServiceController,
    controllerAs: '$',
    templateUrl: template,
};

export default SelectServiceComponent;
