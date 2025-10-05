using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Contracts;


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

            services.AddScoped<IBuildLayoutService, BuildLayoutService>();
            services.AddScoped<IMergeResourcesService, MergeResourcesService>();

            services.AddScoped<ITypeBuilder, TypeBuilder>();
        }
    }
}
