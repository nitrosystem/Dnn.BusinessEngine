using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using System.Web.Routing;
using System.Data;
using System.Data.SqlClient;
using NitroSystem.Dnn.BusinessEngine.Core.WebSocketServer;
using System.Net.WebSockets;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;
using System;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;


namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDbConnection>(sp =>
            {
                var connection = new SqlConnection(DataProvider.Instance().ConnectionString + ";MultipleActiveResultSets=True;");
                connection.Open(); // تضمین می‌کنه که بلافاصله آماده استفاده هست
                return connection;
            });
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<ICacheService, CacheServiceBase>();

            //the services for studio controller
            services.AddScoped<IGlobalService, GlobalService>();
            services.AddScoped<IDefinedListService, DefinedListService>();

            services.AddScoped<IDashbaordService, DashboardService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IActionService, ActionService>();
            services.AddScoped<ITemplateService, TemplateService>();

            services.AddScoped<IBuildModuleService, BuildModuleService>();
            services.AddScoped<IBuildModuleLayout, BuildModuleLayout>();
            services.AddScoped<IModuleBuildLockService, ModuleBuildLockService>();

            services.AddTransient<IResourceMachine, ResourceMachine>();
            services.AddTransient<IProgress<string>, Progress<string>>();

            //GlobalConfiguration.Configuration.Filters.Add(new BasicAuthenticationAttribute());
        }
    }
}
