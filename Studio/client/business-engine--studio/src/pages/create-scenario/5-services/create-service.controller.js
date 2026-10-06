import Swal from 'sweetalert2';
import { GlobalHelper, UrlHelper } from '../../../shared/helpers';
import { GlobalUtil } from '../../../shared/utils';

export class CreateServiceController {
    constructor(
        $rootScope,
        $scope,
        $compile,
        $timeout,
        $q,
        apiService,
        validationService,
        notificationService,
        baseOptions
    ) {
        'ngInject';

        this.$rootScope = $rootScope;
        this.$scope = $scope;
        this.$compile = $compile;
        this.$timeout = $timeout;
        this.$q = $q;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.baseOptions = baseOptions;

        this.stepsCallback = {
            1: this._getServiceTypes,
            2: this._getService,
            3: this._initServiceBuilder
        };
        this.serviceBuilder = {};
        this.service = {
            ScenarioId: this.baseOptions.scenarioId
        };

        const currentTabKey = this.$rootScope.currentTab.key;
        $scope.$on(`onTab--${currentTabKey}--Selected`, (e, args) => {
            this.onFocusModule();
        });

        this.onPageLoad();
    }

    //#region Event Methods

    onPageLoad() {
        this.reloadService = true;

        const id = UrlHelper.getParameterByName('id');
        const step = parseInt(UrlHelper.getParameterByName('st'), 10);
        this.step = !id
            ? 1
            : (step > 1 ? step : 3);
        this._gotoStep(this.step);

        this.onFocusModule();
        this._setForm();
    }

    onFocusModule() {
        this.$rootScope.explorerExpandedItems.push(
            ...['services', 'create-service']
        );
        this.$rootScope.explorerCurrentItem = !this.service || !this.service.Id ?
            'create-service' :
            this.service.Id;
    }

    onSetValidNameClick() {
        this.service.ServiceName = GlobalHelper.normalizeName(this.service.ServiceName);
    }

    onSelectServiceTypeClick(serviceType) {
        if (serviceType && this.serviceType && serviceType.ServiceType === this.serviceType.ServiceType) return;

        this.reloadService = true;
        this.service = {
            ScenarioId: this.baseOptions.scenarioId,
            ServiceType: serviceType.ServiceType,
            HasResult: serviceType.HasResult,
            ResultType: serviceType.ResultType,
        };

        this.serviceType = serviceType;
        this._gotoStep(2);
    }

    onStepClick(step) {
        if (!this.running) this._gotoStep(step);
    }

    onPreviousStepClick() {
        this._gotoStep(this.step - 1);
    }

    onNextStepClick() {
        this._gotoStep(this.step + 1);
    }

    onSaveServiceClick() {
        if (this._validateCurrentStep()) {
            const defer = this.$q.defer();
            this.$scope.$broadcast('onValidateService_' + this.service.ServiceType, defer);
            defer.promise.then((isValid) => {
                if (isValid) {
                    this.running = 'save-service';
                    this.awaitAction = {
                        title: 'Saving Service',
                        subtitle: 'Just a moment for saving the service...',
                    };

                    this.currentTabKey = this.$rootScope.currentTab?.key;

                    this.apiService.post('Studio', 'SaveService', {
                        Service: this.service,
                        ExtensionServiceJson: JSON.stringify(this.extensionService ?? {})
                    }).then((data) => {
                        if (!this.service.Id)
                            this.notifyService.success('Service has been created successfully.');
                        else
                            this.notifyService.info('Service has been updated successfully.');

                        this.service.Id = data.ServiceId;

                        if (data.ExtensionServiceId) this.extensionService.Id = data.ExtensionServiceId;

                        this.$scope.$emit('onUpdateCurrentTab', {
                            id: this.service.Id,
                            title: this.service.ServiceName,
                            key: this.currentTabKey,
                        });

                        this.$rootScope.refreshSidebarExplorerItems();

                        this.awaitAction = null;
                        this.running = null;
                    }, (error) => {
                        this.awaitAction.isError = true;
                        this.awaitAction.subtitle = error.statusText;
                        this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                        this.notifyService.error(error.data);

                        this.running = null;
                    });
                }
            });
        }
    }

    onDeleteServiceClick() {
        let timerInterval;
        Swal.fire({
            title: 'Are you sure?',
            html: '<p>Once deleted, you will not be able to recover this imaginary entity!</p><b></b>',
            icon: 'warning',
            timer: 5000,
            timerProgressBar: true,
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!',
            backdrop: false,
            didOpen: () => {
                const timer = Swal.getPopup().querySelector('b');
                timerInterval = setInterval(() => {
                    timer.textContent = `${Swal.getTimerLeft()}`;
                }, 100);
            },
            willClose: () => {
                clearInterval(timerInterval);
            }
        }).then((result) => {
            if (result.isConfirmed) {
                this.running = 'delete-service';
                this.awaitAction = {
                    title: 'Delete Service',
                    subtitle: 'Just a moment for deleting service...',
                };

                this.apiService.post('Studio', 'DeleteService', { Id: this.service.Id }).then((data) => {
                    if (data) {
                        this.notifyService.info('Service has been deleted successfully.');
                        this.$rootScope.refreshSidebarExplorerItems();
                        this.onCloseWindow();
                    }

                    this.awaitAction = null;
                    this.running = null;
                }, (error) => {
                    this.awaitAction.isError = true;
                    this.awaitAction.subtitle = error.statusText;
                    this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                    this.notifyService.error(error.data);

                    this.running = null;
                });
            }
        });
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }

    //#endregion

    //#region Private Methods

    _setForm() {
        this.form = this.validationService.init({
            ServiceType: {
                required: true,
            },
            ServiceName: {
                id: 'txtServiceName' + (this.service.Id ?? ''),
                rule: (value) => {
                    if (this.step > 1 && !value)
                        return false;

                    if (value && /^[A-Za-z][A-Za-z0-9_]*$/.test(value) === false)
                        return 'Service name is not valid';

                    return true;
                },
                required: true,
            },
        }, true,
            this.$scope,
            '$.service'
        );
    }

    _gotoStep(step) {
        if (step === 1 && this.service?.Id) return;

        if (this.step < step) {
            if (this.step <= 2 && !this._validateCurrentStep(true)) return;

            this.step = step;
        } else if (this.step > step) {
            this.step = step;
        }

        if (this.step !== step) return;

        this.stepsCallback[step]?.call(this);

        const newUrl = UrlHelper.replaceUrlParam('st', step);
        UrlHelper.pushState(newUrl);
    }

    _validateCurrentStep(resetValidate) {
        this.form.validated = true;
        this.form.validator(this.service);
        if (!this.form.valid) return false;

        if (resetValidate) this.form.validated = false;
        return true;
    }

    _getServiceTypes() {
        this.running = 'get-service-types';
        this.awaitAction = {
            title: 'Loading Service Types',
            subtitle: 'Just a moment for loading service types...',
        };

        this.apiService.get('Studio', 'GetServiceTypesListItem').then((data) => {
            this.serviceTypes = data;

            this.running = null;
            this.awaitAction = null;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

            this.notifyService.error(error.data);

            this.running = null;
        });
    }

    _getService() {
        const $defer = this.$q.defer();

        if (!this.reloadService)
            $defer.resolve();
        else {
            const id = UrlHelper.getParameterByName('id');

            this.running = 'get-service';
            this.awaitAction = {
                title: 'Loading Service',
                subtitle: 'Just a moment for loading service...',
            };

            this.apiService.get('Studio', 'GetService', {
                scenarioId: this.baseOptions.scenarioId,
                serviceType: this.service.ServiceType,
                ...(id && { serviceId: id })
            }).then((data) => {
                this.extensionService = data.ExtensionService;
                this.extensionDependency = data.ExtensionDependency;

                if (data.Service) {
                    this.service = data.Service;
                    this.serviceType = {
                        Title: this.service.ServiceTypeTitle,
                        ServiceComponent: this.service.ServiceComponent,
                        Icon: this.service.ServiceTypeIcon,
                        Description: this.service.ServiceTypeDescription
                    }

                    this.$scope.$emit('onUpdateCurrentTab', {
                        id: this.service.Id,
                        title: this.service.ServiceName,
                    });
                }

                $defer.resolve();

                this.reloadService = null;
                this.running = null;
                this.awaitAction = null;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = GlobalUtil.renderErrorHtml(error);

                this.notifyService.error(error.data);

                this.running = null;
            });
        }

        return $defer.promise;
    }

    _initServiceBuilder() {
        if (this.reloadService)
            this._getService().then(() => {
                this.$timeout(() => this._renderComponentServiceBuilder());
            });
        else if (!this._isServiceComponentRendered)
            this._renderComponentServiceBuilder()

        this.serviceBuilderInited = true;
    }

    _renderComponentServiceBuilder() {
        const serviceComponent =
            `<${this.serviceType.ServiceComponent} service-controller="$" service="$.service">
             </${this.serviceType.ServiceComponent}>`;

        $('#pnlServiceBuilder' + (this.service.Id ?? '')).html(
            this.$compile(serviceComponent)(this.$scope)
        );

        this._isServiceComponentRendered = true;
    }

    //#endregion
}