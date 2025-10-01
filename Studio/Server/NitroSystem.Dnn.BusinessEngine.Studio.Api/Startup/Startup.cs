using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine;


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
            services.AddScoped<IActionService, ActionService>();
            services.AddScoped<ITemplateService, TemplateService>();

            services.AddScoped<IBuildModule, BuildModule>();
            services.AddScoped<IBuildModuleLayout, BuildModuleLayout>();
            services.AddScoped<IModuleBuildLockService, ModuleBuildLockService>();

            services.AddScoped<ITypeBuilder, TypeBuilder>();

            services.AddTransient<IResourceMachine, ResourceMachine>();
        }
    }
}
