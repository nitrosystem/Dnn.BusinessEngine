import angular from "angular";
import "angular-filter/dist/angular-filter";
import "angular-dragdrop/src/angular-dragdrop";
import "angular-ui-sortable"
import "ng-file-upload/dist/ng-file-upload";
import "./angular-directives/chosen.directive";

//Configs
import { config as appConfig } from "./angular-configs/app.config";

//Services
import { GlobalService } from "./services/global.service";
import { ApiService } from "./services/api.service";
import { ValidationService } from "./services/validation.service";
import { ModuleDesignerService } from "./create-module/5-module-builder/services/module-designer.service.js";

//Factories
import { NotificationService } from "./services/notification.service";
import { EventService } from "./services/event.service";
import { HubService } from "./services/hub.service";
import { StudioService } from "./services/studio.service";

// Directives
import {
    StudioDirective,
    CustomDateDirective,
    CustomResizeableDirective,
    CustomTooltipDirective,
    CustomPopoverDirective,
    CustomModalDirective,
    CustomFocusDirective,
    CustomSidebarDirective,
    EsckeyDirective,
    NotFieldTypeDirective,
    CustomPagingDirective,
    EnterDirective,
    CustomStarRatingDirective,
    AdjectiveClassDirective,
} from "./angular-directives/custom.directive";
import { MonacoEditor } from "./angular-directives/monaco-editor.directive";

// Components
import SidebarExplorerComponent from "./components/studio-components/sidebar-explorer/sidebar-explorer.component";
import ContentWidgetComponent from "./components/studio-components/content-widget/content-widget.component";
import RightWidgetComponent from "./components/studio-components/right-widget/right-widget.component";
import FieldValidationComponent from "./components/studio-components/field-validation/field-validation.js";
import ServiceParamListComponent from "./components/action-service-components/service-param-list/service-param-list.component";
import ServiceParamsComponent from "./components/action-service-components/service-params/service-params.component.js";
import ActionParamListComponent from "./components/action-service-components/action-param-list/action-param-list.component";
import ConditionListComponent from "./components/action-service-components/condition-list/condition-list.component";
import PropertyListComponent from "./components/action-service-components/property-list/property-list.component";
import SelectServiceComponent from "./components/action-service-components/select-service/select-service.component";
import SelectActionComponent from "./components/action-service-components/select-action/select-action.component";
import SelectiveComponent from "./components/selective-component/selective.component";

//Controllers
import { StudioController } from "./studio.controller";
import { CreateScenarioController } from "./scenario-management/scenarios/create-scenario.controller";
import { EntitiesController } from "./scenario-management/entities/entities.controller";
import { CreateEntityController } from "./scenario-management/entities/create-entity.controller";
import { ViewModelsController } from "./scenario-management/view-models/view-models.controller";
import { CreateViewModelController } from "./scenario-management/view-models/create-view-model.controller";
import { ServicesController } from "./scenario-management/services/services.controller";
import { CreateServiceController } from "./scenario-management/services/create-service.controller";

//create module
import { CreateModuleController } from "./create-module/create-module.controller.js";
import { CreateModuleBasicOptionsController } from "./create-module/1-basic-options/basic-options.controller";
import { CreateModuleTemplateController } from "./create-module/2-template/template.controller";
import { CreateModuleVariablesController } from "./create-module/3-variables/variables.controller";
import { CreateModuleLibrariesController } from "./create-module/4-libraries/libraries.controller";
import { CreateModuleModuleBuilderController } from "./create-module/5-module-builder/module-builder.controller.js";
import { CreateModuleCreateActionController } from "./create-module/6-actions/create-action.controller.js";
import { CreateModuleActionsController } from "./create-module/6-actions/actions.controller.js";

import { ProviderSettingsController } from "./providers/provider-settings.controllers";

//providers
import { DeferredBroadcast, DeferredEmit, DeferredEvent, } from "./angular-providers/deferred-events.provider";

const app = angular
    .module("BusinessEngineStudioApp", [require("angular-sanitize"), "angular.filter", "localytics.directives", "ngDragDrop", "ngFileUpload", "ui.sortable"])
    .config(appConfig)
    .provider("$deferredEvent", DeferredEvent)
    .provider("$deferredEmit", DeferredEmit)
    .provider("$deferredBroadcast", DeferredBroadcast)

    .service("globalService", GlobalService)
    .service("apiService", ApiService)
    .service("validationService", ValidationService)
    .service("moduleDesignerService", ModuleDesignerService)

    .factory("notificationService", NotificationService)
    .factory("eventService", EventService)
    .factory("hubService", HubService)
    .factory("studioService", StudioService)
    
    .directive("studio", StudioDirective)
    .directive("ngEnter", EnterDirective)
    .directive("bCustomDate", CustomDateDirective)
    .directive("bCustomResizeable", CustomResizeableDirective)
    .directive("bCustomTooltip", CustomTooltipDirective)
    .directive("bCustomPopover", CustomPopoverDirective)
    .directive("bCustomModal", CustomModalDirective)
    .directive("bCustomFocus", CustomFocusDirective)
    .directive("bCustomSidebar", CustomSidebarDirective)
    .directive("bCustomPaging", CustomPagingDirective)
    .directive("bCustomStarRating", CustomStarRatingDirective)
    .directive("bAdjectiveClass", AdjectiveClassDirective)
    .directive("bEscKey", EsckeyDirective)
    .directive("bNotFieldType", NotFieldTypeDirective)
    .directive("monacoEditor", MonacoEditor)

    .component("bSidebarExplorer", SidebarExplorerComponent)
    .component("bContentWidget", ContentWidgetComponent)
    .component("bRightWidget", RightWidgetComponent)
    
    .component("bFieldValidation", FieldValidationComponent)
    .component("bServiceParamList", ServiceParamListComponent)
    .component("bServiceParams", ServiceParamsComponent)
    .component("bActionParamList", ActionParamListComponent)
    .component("bConditionList", ConditionListComponent)
    .component("bPropertyList", PropertyListComponent)
    .component("bSelectService", SelectServiceComponent)
    .component("bSelectAction", SelectActionComponent)
    .component("bSelective", SelectiveComponent)

    .controller("studioController", StudioController)
    .controller("createScenarioController", CreateScenarioController)
    .controller("entitiesController", EntitiesController)
    .controller("createEntityController", CreateEntityController)
    .controller("viewModelsController", ViewModelsController)
    .controller("createViewModelController", CreateViewModelController)
    .controller("servicesController", ServicesController)
    .controller("createServiceController", CreateServiceController)

    .controller("createModuleController", CreateModuleController)
    .controller("createModuleBasicOptionsController", CreateModuleBasicOptionsController)
    .controller("createModuleTemplateController", CreateModuleTemplateController)
    .controller("createModuleVariablesController", CreateModuleVariablesController)
    .controller("createModuleLibrariesController", CreateModuleLibrariesController)
    .controller("createModuleModuleBuilderController", CreateModuleModuleBuilderController)
    .controller("createModuleActionsController", CreateModuleActionsController)
    .controller("createModuleCreateActionController", CreateModuleCreateActionController)

    .controller("providerSettingsController", ProviderSettingsController)

window["app"] = app;

export { app };