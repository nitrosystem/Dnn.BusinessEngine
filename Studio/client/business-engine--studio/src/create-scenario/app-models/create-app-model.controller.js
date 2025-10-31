import { GlobalSettings } from "../../angular/angular-configs/global.settings";

export class CreateAppModelController {
    constructor(
        $scope,
        $rootScope,
        studioService,
        $timeout,
        globalService,
        apiService,
        validationService,
        notificationService
    ) {
        "ngInject";

        this.$scope = $scope;
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.globalService = globalService;
        this.apiService = apiService;
        this.validationService = validationService;
        this.notifyService = notificationService;

        this.groups = _.filter(this.$rootScope.groups, (g) => { return g.ObjectType == 'AppModel' });

        studioService.setFocusModuleDelegate(this, this.onFocusModule);

        this.onPageLoad();
    }

    onPageLoad() {
        const id = this.globalService.getParameterByName("id");

        this.running = "get-appModel";
        this.awaitAction = {
            title: "Loading AppModel",
            subtitle: "Just a moment for loading view model...",
        };

        this.apiService.get("Studio", "GetAppModel", { appModelId: id || null }).then((data) => {
            this.propertyTypes = data.PropertyTypes;
            this.appModels = data.AppModels;
            this.appModel = data.AppModel;
            if (!this.appModel) {
                this.appModel = {
                    ScenarioId: GlobalSettings.scenarioId,
                };
            } else {
                this.$scope.$emit("onUpdateCurrentTab", {
                    id: this.appModel.Id,
                    title: this.appModel.ModelName,
                });
            }

            this.onFocusModule();

            delete this.running;
            delete this.awaitAction;
        }, (error) => {
            this.awaitAction.isError = true;
            this.awaitAction.subtitle = error.statusText;
            this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

            this.notifyService.error(error.data.Message);

            delete this.running;
        });

        this.setForm();
    }

    onFocusModule() {
        this.$rootScope.explorerExpandedItems.push(...["app-models", "create-app-model"]);
        this.$rootScope.explorerCurrentItem = !this.appModel || !this.appModel.Id ?
            "create-app-model" :
            this.appModel.Id;
    }

    setForm() {
        this.form = this.validationService.init({
            ScenarioId: {
                rule: "guid",
                id: "drpScenarioId",
                required: true,
            },
            ModelName: {
                id: "txtModelName",
                rule: (value) => {
                    if (/^[A-Z][A-Za-z0-9]*$/.test(value) == false)
                        return "Entity name is not valid";
                    else
                        return true;
                },
                required: true,
            },
            Properties: {
                rule: (value) => {
                    if (value && value.length >= 1) return true;

                    return "App Model must have properties";
                },
                required: true,
            },
        },
            true,
            this.$scope,
            "$.appModel"
        );

        this.propertyForm = this.validationService.init({
            PropertyName: {
                id: "txtPropertyName",
                required: true,
            },
            PropertyType: {
                id: "drpPropertyType",
                required: true,
            },
            PropertyTypeId: {
                rule: (value) => {
                    if (
                        (this.property.PropertyType == "appModel" ||
                            this.property.PropertyType == "listOfAppModel") &&
                        !value
                    ) {
                        return "Select a view model for property type";
                    } else return true;
                },
                id: "drpPropertyTypeId",
            },
        });
    }

    onSetValidNameClick() {
        this.appModel.ModelName = this.globalService.normalizeName(this.appModel.ModelName);
    }

    /*------------------------------------
           AppModel Property
          ------------------------------------*/
    onAddPropertyClick() {
        if (this.property) return;

        this.appModel.Properties = this.appModel.Properties || [];

        const propertyIndex = this.appModel.Properties.length;
        const property = {
            IsEdited: true,
            IsNew: true,
            AllowNulls: true,
            ViewOrder: propertyIndex + 1,
        };

        this.appModel.Properties.push(property);

        this.property = _.clone(property);

        this.propertyIndex = propertyIndex;

        this.focusPropertyForm();
    }

    onRowItemClick(property, index) {
        if (this.property) return;

        property.IsEdited = true;

        this.property = _.clone(property);
        this.property.IsNew = false;

        this.propertyIndex = index;

        this.focusPropertyForm();
    }

    focusPropertyForm() {
        this.$timeout(() => {
            this.$scope.$broadcast("onEditProperty");
        });
    }

    onPropertySwapClick(index, swappedIndex) {
        const properties = this.appModel.Properties;

        if (swappedIndex > -1 && swappedIndex < properties.length) {
            [properties[index], properties[swappedIndex]] = [
                properties[swappedIndex],
                properties[index],
            ];

            properties.map(
                (c) => (c.ViewOrder = this.appModel.Properties.indexOf(c) + 1)
            );
        }
    }

    onSavePropertyClick() {
        this.propertyForm.validated = true;
        this.propertyForm.validator(this.property);
        if (this.propertyForm.valid) {
            this.property.IsEdited = false;
            this.appModel.Properties[this.propertyIndex] = _.clone(this.property);

            delete this.property;
            delete this.propertyIndex;
        }
    }

    onCancelPropertyClick() {
        if (this.property.IsNew)
            this.appModel.Properties.splice(this.propertyIndex, 1);
        else this.appModel.Properties[this.propertyIndex].IsEdited = false;

        delete this.property;
        delete this.propertyIndex;
    }

    onSaveAppModelClick() {
        this.form.validated = true;
        this.form.validator(this.appModel);

        if (this.form.valid) {
            this.running = "save-appModel";
            this.awaitAction = {
                title: "Creating AppModel",
                subtitle: "Just a moment for creating appModel...",
            };

            this.currentTabKey = this.$rootScope.currentTab.key;

            this.apiService.post("Studio", "SaveAppModel", this.appModel).then((data) => {
                this.appModel = data;

                this.notifyService.success("AppModel updated has been successfully");

                this.$scope.$emit("onUpdateCurrentTab", {
                    id: this.appModel.Id,
                    title: this.appModel.ModelName,
                    key: this.currentTabKey,
                });

                this.$rootScope.refreshSidebarExplorerItems();

                delete this.awaitAction;
                delete this.running;
            }, (error) => {
                this.awaitAction.isError = true;
                this.awaitAction.subtitle = error.statusText;
                this.awaitAction.desc = this.globalService.getErrorHtmlFormat(error);

                if (error.data.HResult == "-2146232060")
                    this.notifyService.error(
                        `AppModel name must be unique.${this.appModel.ModelName} is already in the scenario appModels`
                    );
                else if (error.data.HResult == "-2146233088")
                    this.notifyService.error(
                        `Table name must be unique.${this.appModel.TableName} is already in the database`
                    );
                else this.notifyService.error(error.data.Message);

                delete this.running;
            }
            );
        }
    }

    onDeleteAppModelClick() {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this imaginary view model!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((willDelete) => {
            if (willDelete) {
                this.running = "get-appModels";
                this.awaitAction = {
                    title: "Remove AppModel",
                    subtitle: "Just a moment for removing appModel...",
                };

                this.apiService.post("Studio", "DeleteAppModel", { Id: this.appModel.Id }).then((data) => {
                    if (data) this.notifyService.success("AppModel deleted has been successfully");

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