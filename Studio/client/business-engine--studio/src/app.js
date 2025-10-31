import angular from "angular";
import "angular-filter/dist/angular-filter";
import "angular-dragdrop/src/angular-dragdrop";
import "angular-ui-sortable"
import "ng-file-upload/dist/ng-file-upload";
import "./angular/angular-directives/chosen.directive";

//configs
import { config as appConfig } from "./angular/angular-configs/app.config";

//angular providers
import { DeferredBroadcast, DeferredEmit, DeferredEvent, } from "./angular/angular-providers/deferred-events.provider";

//services
import { GlobalService } from "./services/global.service";
import { ApiService } from "./services/api.service";
import { ValidationService } from "./services/validation.service";
import { NotificationService } from "./services/notification.service";
import { EventService } from "./services/event.service";
import { StudioService } from "./services/studio.service";
import { ModuleDesignerService } from "./create-module/5-module-builder/services/module-designer.service";

//directives
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
} from "./angular/angular-directives/custom.directive";
import { MonacoEditor } from "./angular/angular-directives/monaco-editor.directive";

// Components
import SidebarExplorerComponent from "./angular/components/studio-components/sidebar-explorer/sidebar-explorer.component";
import ContentWidgetComponent from "./angular/components/studio-components/content-widget/content-widget.component";
import RightWidgetComponent from "./angular/components/studio-components/right-widget/right-widget.component";
import ServiceParamsComponent from "./angular/components/action-service-components/service-params/service-params.component";
import ActionParamListComponent from "./angular/components/action-service-components/action-param-list/action-param-list.component";
import ConditionListComponent from "./angular/components/action-service-components/condition-list/condition-list.component";
import PropertyListComponent from "./angular/components/action-service-components/property-list/property-list.component";
import SelectServiceComponent from "./angular/components/action-service-components/select-service/select-service.component";
import SelectActionComponent from "./angular/components/action-service-components/select-action/select-action.component";
import SelectiveComponent from "./angular/components/selective-component/selective.component";

//Controllers
import { StudioController } from "./studio.controller";
import { CreateScenarioController } from "./create-scenario/scenarios/create-scenario.controller";
import { EntitiesController } from "./create-scenario/entities/entities.controller";
import { CreateEntityController } from "./create-scenario/entities/create-entity.controller";
import { AppModelsController } from "./create-scenario/app-models/app-models.controller";
import { CreateAppModelController } from "./create-scenario/app-models/create-app-model.controller";
import { ServicesController } from "./create-scenario/services/services.controller";
import { CreateServiceController } from "./create-scenario/services/create-service.controller";
import { ExtensionsController } from "./extensions/extensions.controller";

//create dashboard
import { CreateDashboardController } from "./create-dashboard/create-dashboard.controller";
import { CreateDashboardBasicOptionsController } from "./create-dashboard/1-basic-options/basic-options.controller";
import { CreateDashboardSkinController } from "./create-dashboard/2-appearance/skin.controller";
import { CreateDashboardPagesController } from "./create-dashboard/3-pages/pages.controllers";
import { CreateDashboardPageController } from "./create-dashboard/3-pages/create-page.controller";
import { CreateDashboardModulesController } from "./create-dashboard/4-modules/modules.controller";

//create module
import { CreateModuleController } from "./create-module/create-module.controller";
import { CreateModuleBasicOptionsController } from "./create-module/1-basic-options/basic-options.controller";
import { CreateModuleTemplateController } from "./create-module/2-template/template.controller";
import { CreateModuleLibrariesController } from "./create-module/3-libraries/libraries.controller";
import { CreateModuleVariablesController } from "./create-module/4-variables/variables.controller";
import { CreateModuleModuleBuilderController } from "./create-module/5-module-builder/module-builder.controller";
import { CreateModuleActionsController } from "./create-module/6-actions/actions.controller";
import { CreateModuleCreateActionController } from "./create-module/6-actions/create-action.controller";

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
    .controller("appModelsController", AppModelsController)
    .controller("createAppModelController", CreateAppModelController)
    .controller("servicesController", ServicesController)
    .controller("createServiceController", CreateServiceController)
    .controller("extensionsController", ExtensionsController)

    .controller("createDashboardController", CreateDashboardController)
    .controller("createDashboardBasicOptionsController", CreateDashboardBasicOptionsController)
    .controller("createDashboardSkinController", CreateDashboardSkinController)
    .controller("createDashboardPagesController", CreateDashboardPagesController)
    .controller("createDashboardPageController", CreateDashboardPageController)
    .controller("createDashboardModulesController", CreateDashboardModulesController)

    .controller("createModuleController", CreateModuleController)
    .controller("createModuleBasicOptionsController", CreateModuleBasicOptionsController)
    .controller("createModuleTemplateController", CreateModuleTemplateController)
    .controller("createModuleVariablesController", CreateModuleVariablesController)
    .controller("createModuleLibrariesController", CreateModuleLibrariesController)
    .controller("createModuleModuleBuilderController", CreateModuleModuleBuilderController)
    .controller("createModuleActionsController", CreateModuleActionsController)
    .controller("createModuleCreateActionController", CreateModuleCreateActionController)

window["app"] = app;

export { app };