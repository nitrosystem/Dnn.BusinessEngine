using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Action;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Template;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Service;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.AppModel;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Entity;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Base;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services;
using NitroSystem.Dnn.BusinessEngine.Core.BrtPath.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.BrtPath;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Extension;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Workflow;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Workflow;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<IEntityService, EntityService>();
            services.AddScoped<IAppModelService, AppModelService>();
            services.AddScoped<IServiceFactory, ServiceFactory>();
            services.AddScoped<IBrtGateService, InMemoryBrtGate>();

            services.AddScoped<IExtensionService, ExtensionService>();
            services.AddScoped<IDefinedListService, DefinedListService>();

            services.AddScoped<IDashboardService, DashboardService>();

            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IModuleTemplateService, ModuleTemplateService>();
            services.AddScoped<IModuleFieldService, ModuleFieldService>();
            services.AddScoped<IModuleVariableService, ModuleVariableService>();
            services.AddScoped<IModuleLibraryAndResourceService, ModuleLibraryAndResourceService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IActionService, ActionService>();
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<IWorkflowEventService, WorkflowEventService>();

            services.AddScoped<IBuildLayoutService, BuildLayoutService>();
            services.AddScoped<IMergeResourcesService, MergeResourcesService>();
            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, InitializeBuildModuleMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, DeleteOldResourcesMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, BuildLayoutMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, MergeResourcesMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, ResourceAggregatorMiddleware>();
            services.AddScoped<InitializeBuildModuleMiddleware>();
            services.AddScoped<DeleteOldResourcesMiddleware>();
            services.AddScoped<BuildLayoutMiddleware>();
            services.AddScoped<MergeResourcesMiddleware>();
            services.AddScoped<ResourceAggregatorMiddleware>();
            services.AddScoped<BuildModuleRunner>();

            services.AddScoped<IEngineMiddleware<BuildTypeRequest, BuildTypeResponse>, InitializeBuildTypeMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildTypeRequest, BuildTypeResponse>, BuildTypeMiddleware>();
            services.AddScoped<InitializeBuildTypeMiddleware>();
            services.AddScoped<BuildTypeMiddleware>();
            services.AddScoped<BuildTypeRunner>();

            //services.AddScoped<IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>, SqlDataProviderMiddleware>();
            //services.AddScoped<IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>, ResourcesMiddleware>();
            //services.AddScoped<SqlDataProviderMiddleware>();
            //services.AddScoped<ResourcesMiddleware>();

            BaseMappingProfile.Register();
            EntityMappingProfile.Register();
            AppModelMappingProfile.Register();
            ServiceMappingProfile.Register();
            ModuleMappingProfile.Register();
            DashboardMappingProfile.Register();
            ActionMappingProfile.Register();
            TemplateMappingProfile.Register();
            ExtensionMappingProfile.Register();
        }
    }
}
