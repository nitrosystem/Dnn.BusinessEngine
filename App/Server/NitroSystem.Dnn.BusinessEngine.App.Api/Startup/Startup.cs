using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using System.Data.SqlClient;
using System.Data;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Services;

namespace NitroSystem.Dnn.BusinessEngine.Api.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddScoped<IModuleData, ModuleData>();

            //GlobalConfiguration.Configuration.Filters.Add(new BasicAuthenticationAttribute());
            services.AddScoped<IDbConnection>(sp =>
            {
                var connection = new SqlConnection(DataProvider.Instance().ConnectionString + ";MultipleActiveResultSets=True;");
                connection.Open(); // تضمین می‌کنه که بلافاصله آماده استفاده هست
                return connection;
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<ICacheService, CacheServiceBase>();

            //services.AddScoped<IDashbaordService, DashboardService>();
            services.AddScoped<IModuleService, ModuleService>();
            //services.AddScoped<IActionService, ActionService>();
        }
    }
}
