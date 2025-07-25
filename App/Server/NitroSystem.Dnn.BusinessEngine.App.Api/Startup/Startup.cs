using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using System.Data.SqlClient;
using System.Data;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System.Web.Routing;
using System.Net.WebSockets;
using System;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using NitroSystem.Dnn.BusinessEngine.Core.ModuleBuilder;
using NitroSystem.Dnn.BusinessEngine.Core.ModuleData;
using NitroSystem.Dnn.BusinessEngine.App.Framework.ModuleData;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Services;
using NitroSystem.Dnn.BusinessEngine.App.Framework.Services;


namespace NitroSystem.Dnn.BusinessEngine.Api.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServiceLocator, ServiceLocator>();
            services.AddSingleton<ICacheService, CacheServiceBase>();

            services.AddScoped<IDbConnection>(sp =>
            {
                var connection = new SqlConnection(DataProvider.Instance().ConnectionString + ";MultipleActiveResultSets=True;");
                connection.Open();
                return connection;
            });
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRepositoryBase, RepositoryBase>();

            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IActionService, ActionService>();

            services.AddScoped<IModuleData, ModuleData>();
            services.AddScoped<IExpressionService, ExpressionService>();

            services.AddScoped<IActionCondition, ActionCondition>();
            services.AddScoped<IActionWorker, ActionWorker>();
            //services.AddScoped<IServiceWorker, ServiceWorker>();
        }
    }
}
