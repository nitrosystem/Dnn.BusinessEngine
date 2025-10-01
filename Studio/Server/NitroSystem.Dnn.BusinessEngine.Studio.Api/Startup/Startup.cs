using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.AppModelTypeBuilder;


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

            services.AddScoped<ITypeBuilderService, TypeBuilderService>();

            services.AddScoped<IBuildModuleService, BuildModuleService>();
            services.AddScoped<IBuildModuleLayout, BuildModuleLayout>();
            services.AddScoped<IModuleBuildLockService, ModuleBuildLockService>();

            services.AddTransient<IResourceMachine, ResourceMachine>();
        }
    }
}
