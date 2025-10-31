import { GlobalSettings } from "../../angular/angular-configs/global.settings";

export class CreateServiceController {
    constructor(
        $scope,
        $rootScope,
        studioService,
        $compile,
        $timeout,
        $q,
        globalService,
        apiService,
        validationService,
        notificationService,
        $deferredBroadcast
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$compile = $compile;
        this.$timeout = $timeout;
        this.$q = $q;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;
        this.$deferredBroadcast = $deferredBroadcast;

        this.groups = _.filter(this.$rootScope.groups, (g) => { return g.ObjectType == 'Service' });
        this.stepsValid = 1;
        this.stepsCallback = {
            1: this.getServiceTypes,
            2: this.getService,
            3: this.initServiceBuilder
        };
        this.serviceBuilder = {};
        this.service = {
            ScenarioId: GlobalSettings.scenarioId
        };

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        this.reloadServiceTypes = true;
        this.reloadService = true;

        const id = this.globalService.getParameterByName("id");
        const step = this.globalService.getParameterByName("st");

        this.step = id
            ? (step > 1 ? parseInt(step) : 2)
            : 1;
        this.gotoStep(this.step);

        this.onFocusModule();
        this.setForm();
    }

    onFocusModule() {
        this.$rootScope.explorerExpandedItems.push(
            ...["services", "create-service"]
        );
        this.$rootScope.explorerCurrentItem = !this.service || !this.service.Id ?
            "create-service" :
            this.service.Id;
    }

    setForm() {
        this.form = this.validationService.init({
            ScenarioId: {
                id: "drpScenarioId" +
                    (this.service.Id ? this.service.Id : ""),
                rule: (value) => {
                    if (this.step > 1 && !value) return false;

                    return true;
                },
                required: true,
            },
            ServiceType: {
                required: true,
            },
            ServiceName: {
                id: "txtServiceName" +
                    (this.service.Id ? this.service.Id : ""),
                rule: (value) => {
                    if (this.step > 1 && !value) return false;

                    return true;
                },
                required: true,
            },
        }, true,
            this.$scope,
            "$.service"
        );
    }

    onSetValidNameClick() {
        this.service.ServiceName = this.globalService.normalizeName(this.service.ServiceName);
    }

    gotoStep(step) {
        if (this.step < step) {
            if (this.step == 2) {
                this.form.validated = true;
                this.form.validator(this.service);

                if (this.form.valid) {
                    this.step = step;
                    this.form.validated = false;
                    this.stepsValid = this.step;
                }
            }
            else
                this.step = step;
        } else if (this.step > step) {
            this.step = step;
        }

        if (this.step == step) {
            if (typeof this.stepsCallback[step] == "function") {
                this.stepsCallback[step].apply(this);
            }

            var newUrl = this.globalService.replaceUrlParam("st", step);
            this.globalService.pushState(newUrl);
        }
    }

    getServiceTypes() {
        const $defer = this.$q.defer();

        if (!this.reloadServiceTypes || this.service.Id)
            $defer.resolve();
        else {
            this.running = "get-service-types";
            this.awaitAction = {
                title: "Loading Service Types",
                subtitle: "Just a moment for loading service types...",
            };

            this.apiService.get("Studio", "GetServiceTypesListItem").then((data) => {
                this.serviceTypes = data;

                $defer.resolve();

                delete this.reloadServiceTypes;
                delete this.running;
                delete this.awaitAction;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }

        return $defer.promise;
    }

    getService() {
        const $defer = this.$q.defer();

        if (!this.reloadService)
            $defer.resolve();
        else {
            const id = this.globalService.getParameterByName("id");

            this.running = "get-service";
            this.awaitAction = {
                title: "Loading Service",
                subtitle: "Just a moment for loading service...",
            };

            this.apiService.get("Studio", "GetService", {
                serviceType: this.service.ServiceType || '',
                ...(id && { serviceId: id })
            }).then((data) => {
                this.extensionService = data.ExtensionService;
                this.extensionDependency = data.ExtensionDependency;

                if (data.Service) {
                    this.service = data.Service;
                    this.serviceType = {
                        Title: this.service.ServiceTypeTitle,
                        ServiceComponent: this.service.ServiceComponent,
                        Icon: this.service.ServiceTypeIcon
                    }

                    this.stepsValid = 3;

                    this.$scope.$emit("onUpdateCurrentTab", {
                        id: this.service.Id,
                        title: this.service.ServiceName,
                    });
                }

                $defer.resolve();

                delete this.reloadService;
                delete this.running;
                delete this.awaitAction;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                this.notifyService.error(error.data.Message);

                delete this.running;
            });
        }

        return $defer.promise;
    }

    initServiceBuilder() {
        const $defer = this.$q.defer();

        if (this.reloadService)
            this.getService().then(() => {
                this.$timeout(() => this.renderComponentServiceBuilder());
            });
        else
            this.renderComponentServiceBuilder()

        $defer.resolve();

        return $defer.promise;
    }

    renderComponentServiceBuilder() {
        const serviceComponent =
            `<${this.serviceType.ServiceComponent} service-controller="$" service="$.service">
             </${this.serviceType.ServiceComponent}>`;

        $("#pnlServiceBuilder" + (this.service.Id ? this.service.Id : "")).html(
            this.$compile(serviceComponent)(this.$scope)
        );
    }

    onSelectServiceTypeClick(serviceType) {
        this.reloadService = true;
        this.service = {
            ScenarioId: GlobalSettings.scenarioId,
            ServiceType: serviceType.ServiceType,
            HasResult: serviceType.HasResult,
            ResultType: serviceType.ResultType,
        };

        this.serviceType = serviceType;

        if (this.stepsValid == 1) {
            this.stepsValid = 2;
            this.gotoStep(2);
        }
    }

    onStepClick(step) {
        if (!this.running && this.stepsValid >= step) this.gotoStep(step);
    }

    onPrevStepClick() {
        this.gotoStep(this.step - 1);
    }

    onNextStepClick() {
        this.gotoStep(this.step + 1);
    }

    onSaveServiceClick() {
        this.form.validated = true;
        this.form.validator(this.service);
        if (this.form.valid) {
            this.$deferredBroadcast(this.$scope, "onValidateService").then(
                (isValid) => {
                    if (isValid) {
                        this.running = "save-service";
                        this.awaitAction = {
                            title: "Saving Service",
                            subtitle: "Just a moment for saving the service...",
                        };

                        this.currentTabKey = this.$rootScope.currentTab.key;

                        this.apiService.post("Studio", "SaveService", {
                            Service: this.service,
                            ExtensionServiceJson: JSON.stringify(this.extensionService ?? {})
                        }).then((data) => {
                            this.service.Id = data.ServiceId;

                            if (data.ExtensionServiceId) this.extensionService.Id = data.ExtensionServiceId;

                            this.notifyService.success("Service updated has been successfully");

                            this.$scope.$emit("onUpdateCurrentTab", {
                                id: this.service.Id,
                                title: this.service.ServiceName,
                                key: this.currentTabKey,
                            });

                            this.$rootScope.refreshSidebarExplorerItems();

                            delete this.awaitAction;
                            delete this.running;
                        }, (error) => {
                            this.awaitAction.isError = true;
                            this.awaitAction.subtitle = error.statusText;
                            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                            if (error.data) this.notifyService.error(error.data.Message);

                            delete this.running;
                        });
                    }
                }
            );
        }
    }

    onCancelServiceClick() {
        $("#wnCreateService").modal("hide");

        delete this.service;

        this.$scope.serviceForm.$submitted = false;

        location.reload();
    }

    onDeleteServiceClick() {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this imaginary service!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((willDelete) => {
            if (willDelete) {
                this.running = "get-services";
                this.awaitAction = {
                    title: "Remove Service",
                    subtitle: "Just a moment for removing service...",
                };

                this.apiService.post("Studio", "DeleteService", { Id: this.service.Id }).then((data) => {
                    if (data) this.notifyService.success("Service deleted has been successfully");

                    this.onCloseWindow();

                    this.$rootScope.refreshSidebarExplorerItems();

                    delete this.awaitAction;
                    delete this.running;
                }, (error) => {
                    this.awaitAction.isError = true;
                    this.awaitAction.subtitle = error.statusText;
                    this.awaitAction.desc =
                        this.globalService.getErrorHtmlFormat(error);

                    this.notifyService.error(error.data.Message);

                    delete this.running;
                });
            }
        });
    }

    onCloseWindow() {
        this.$scope.$emit('onCloseModule');
    }
}