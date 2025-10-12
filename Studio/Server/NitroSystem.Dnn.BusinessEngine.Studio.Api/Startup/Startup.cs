using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Module;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Action;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Template;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Service;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.AppModel;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Entity;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services;


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

            services.AddScoped<IDefinedListService, DefinedListService>();

            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IModuleFieldService, ModuleFieldService>();
            services.AddScoped<IModuleVariableService, ModuleVariableService>();
            services.AddScoped<IModuleLibraryAndResourceService, ModuleLibraryAndResourceService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IActionService, ActionService>();
            services.AddScoped<ITemplateService, TemplateService>();

            services.AddScoped<IBuildLayoutService, BuildLayoutService>();
            services.AddScoped<IMergeResourcesService, MergeResourcesService>();
            services.AddScoped<IResourceAggregatorService, ResourceAggregatorService>();

            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, BuildLayoutMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, MergeResourcesMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, ResourceAggregatorMiddleware>();

            services.AddScoped<BuildLayoutMiddleware>();
            services.AddScoped<MergeResourcesMiddleware>();
            services.AddScoped<ResourceAggregatorMiddleware>();

            services.AddScoped<ITypeBuilder, TypeBuilder>();

            BaseMappingProfile.Register();
            EntityMappingProfile.Register();
            AppModelMappingProfile.Register();
            ServiceMappingProfile.Register();
            ModuleMappingProfile.Register();
            ActionMappingProfile.Register();
            TemplateMappingProfile.Register();
        }
    }
}
