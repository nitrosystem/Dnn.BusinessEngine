using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using DotNetNuke.Data;
using System.Data.SqlClient;
using System.Data;
using System.Web.Routing;
using System.Net.WebSockets;
using System;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using System.Reflection;
using System.Linq;
using NitroSystem.Dnn.BusinessEngine.App.Framework.ModuleData;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Database;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DnnServices;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var actionTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IExtensionServiceFactory).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in actionTypes)
                services.AddScoped(type);

            services.AddTransient<IDataSourceService, DataSourceService>();
            services.AddTransient<ILoginUserService, LoginUserService>();
        }
    }
}
