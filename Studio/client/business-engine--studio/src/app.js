import angular from 'angular';

//configs
import { config as appConfig } from './app.config';

//constants
import { BaseOptionsConstants, SqlConstants } from './shared/constants';

//filters
import { GroupBy, ToArray } from './shared/filters';

//services
import { ApiService } from './shared/services/api.service';
import { ValidationService } from './shared/services/validation.service';
import { ActionCenterService } from './shared/services/action-center.service';
import { NotificationService } from './shared/services/notification.service';
import { SseClientService } from './shared/services/sse-client.service';

//directives
import { MonacoHtmlEditor, MonacoPropertiesEditor, MonacoDslEditor } from './shared/directives/monaco-editor';
import { ChosenDropdownDirective } from './shared/directives/chosen-dropdown/chosen-dropdown.directive';
import { BootstrapModalDirective, BootstrapTooltipDirective, BootstrapPopoverDirective } from './shared/directives/bootstrap';
import { StudioDirective, DateFormatDirective, FocusDirective, SidebarDirective, EsckeyDirective, EnterKeyDirective } from './shared/directives/global';
import { PagingDirective } from './shared/directives/paging/paging.directive';
import { registerBPropertiesLanguage } from './shared/directives/monaco-editor/properties-editor/register-language';
import { registerBPropertiesCompletion } from './shared/directives/monaco-editor/properties-editor/completion';

// Components
import SidebarExplorerComponent from './shared/components/sidebar-explorer/sidebar-explorer.component';
import ContentWidgetComponent from './shared/components/widgets/content-widget/content-widget.component';
import RightWidgetComponent from './shared/components/widgets/right-widget/right-widget.component';
import ServiceParamsComponent from './shared/components/service-params/service-params.component';
import ActionParamListComponent from './shared/components/action-params/action-params.component';
import SelectServiceComponent from './shared/components/select-service/select-service.component';
import SelectiveComponent from './shared/components/selective/selective.component';

//Controllers

//global
import { StudioController } from './pages/studio.controller';
import { ExtensionsController } from './pages/extensions/extensions.controller';

//create scenario
import { SelectScenarioController } from './pages/create-scenario/1-scenarios/select-scenario.controller';
import { CreateScenarioController } from './pages/create-scenario/1-scenarios/create-scenario.controller';
import { CreateGroupController } from './pages/create-scenario/2-groups/create-group.controller';
import { EntitiesController } from './pages/create-scenario/3-entities/entities.controller';
import { CreateEntityController } from './pages/create-scenario/3-entities/create-entity.controller';
import { AppModelsController } from './pages/create-scenario/4-app-models/app-models.controller';
import { CreateAppModelController } from './pages/create-scenario/4-app-models/create-app-model.controller';
import { ServicesController } from './pages/create-scenario/5-services/services.controller';
import { CreateServiceController } from './pages/create-scenario/5-services/create-service.controller';

//create dashboard
import { CreateDashboardController } from './pages/create-dashboard/create-dashboard.controller';
import { CreateDashboardBasicOptionsController } from './pages/create-dashboard/1-basic-options/basic-options.controller';
import { CreateDashboardPagesController } from './pages/create-dashboard/2-pages/pages.controllers';
import { CreateDashboardPageController } from './pages/create-dashboard/2-pages/create-page.controller';

//create module
import { CreateModuleController } from './pages/create-module/create-module.controller';
import { CreateModuleBasicOptionsController } from './pages/create-module/1-basic-options/basic-options.controller';
import { CreateModuleTemplateController } from './pages/create-module/2-template/template.controller';
import { CreateModuleLibrariesController } from './pages/create-module/3-libraries/libraries.controller';
import { CreateModuleVariablesController } from './pages/create-module/4-variables/variables.controller';
import { CreateModuleModuleBuilderController } from './pages/create-module/5-module-builder/module-builder.controller';
import { ModuleDesignerService } from './pages/create-module/5-module-builder/module-designer.service';
import { CreateModuleActionsController } from './pages/create-module/6-actions/actions.controller';
import { CreateModuleCreateActionController } from './pages/create-module/6-actions/create-action.controller';

const app = angular
    .module('BusinessEngineStudioApp', [])
    .config(appConfig)

    .constant('sqlConstants', SqlConstants)
    .constant('baseOptions', BaseOptionsConstants)

    .filter('groupBy', GroupBy)
    .filter('toArray', ToArray)

    .service('apiService', ApiService)
    .service('sseClientService', SseClientService)
    .service('notificationService', NotificationService)
    .service('actionCenterService', ActionCenterService)
    .service('moduleDesignerService', ModuleDesignerService)
    .service('validationService', ValidationService)

    .directive('studio', StudioDirective)
    .directive('bootstrapTooltip', BootstrapTooltipDirective)
    .directive('bootstrapPopover', BootstrapPopoverDirective)
    .directive('bootstrapModal', BootstrapModalDirective)
    .directive('chosen', ChosenDropdownDirective)
    .directive('bEnterKey', EnterKeyDirective)
    .directive('bDateFormat', DateFormatDirective)
    .directive('bFocus', FocusDirective)
    .directive('bSidebar', SidebarDirective)
    .directive('bEscKey', EsckeyDirective)
    .directive('bPaging', PagingDirective)
    .directive('htmlEditor', MonacoHtmlEditor)
    .directive('propertiesEditor', MonacoPropertiesEditor)
    .directive('dslEditor', MonacoDslEditor)

    .component('bSidebarExplorer', SidebarExplorerComponent)
    .component('bContentWidget', ContentWidgetComponent)
    .component('bRightWidget', RightWidgetComponent)
    .component('bSelectService', SelectServiceComponent)
    .component('bServiceParams', ServiceParamsComponent)
    .component('bActionParamList', ActionParamListComponent)
    .component('bSelective', SelectiveComponent)

    .controller('studioController', StudioController)
    .controller('extensionsController', ExtensionsController)
    .controller('selectScenarioController', SelectScenarioController)
    .controller('createScenarioController', CreateScenarioController)
    .controller('createGroupController', CreateGroupController)
    .controller('entitiesController', EntitiesController)
    .controller('createEntityController', CreateEntityController)
    .controller('appModelsController', AppModelsController)
    .controller('createAppModelController', CreateAppModelController)
    .controller('servicesController', ServicesController)
    .controller('createServiceController', CreateServiceController)
    .controller('createDashboardController', CreateDashboardController)
    .controller('createDashboardBasicOptionsController', CreateDashboardBasicOptionsController)
    .controller('createDashboardPagesController', CreateDashboardPagesController)
    .controller('createDashboardPageController', CreateDashboardPageController)
    .controller('createModuleController', CreateModuleController)
    .controller('createModuleBasicOptionsController', CreateModuleBasicOptionsController)
    .controller('createModuleTemplateController', CreateModuleTemplateController)
    .controller('createModuleLibrariesController', CreateModuleLibrariesController)
    .controller('createModuleVariablesController', CreateModuleVariablesController)
    .controller('createModuleModuleBuilderController', CreateModuleModuleBuilderController)
    .controller('createModuleActionsController', CreateModuleActionsController)
    .controller('createModuleCreateActionController', CreateModuleCreateActionController)

registerBPropertiesLanguage();
registerBPropertiesCompletion();

window['app'] = app;
// export { app };