using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using System.Web.Routing;
using System.Data;
using System.Data.SqlClient;
using System.Net.WebSockets;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;
using System;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection;


namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IGlobalService, GlobalService>();
            services.AddScoped<IEntityService, EntityService>();
            services.AddScoped<IAppModelService, AppModelService>();
            services.AddScoped<IServiceFactory, ServiceFactory>();
            services.AddScoped<IDefinedListService, DefinedListService>();

            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IActionService, ActionService>();
            services.AddScoped<ITemplateService, TemplateService>();

            services.AddScoped<IBuildModuleService, BuildModuleService>();
            services.AddScoped<IBuildModuleLayout, BuildModuleLayout>();
            services.AddScoped<IModuleBuildLockService, ModuleBuildLockService>();

            services.AddTransient<IResourceMachine, ResourceMachine>();

            //GlobalConfiguration.Configuration.Filters.Add(new BasicAuthenticationAttribute());
        }
    }
}
