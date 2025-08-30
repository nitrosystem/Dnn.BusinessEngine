using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using System.Web.Routing;
using System.Data;
using System.Data.SqlClient;
using System.Net.WebSockets;
using System;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader;

namespace NitroSystem.Dnn.BusinessEngine.Core.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServiceLocator, ServiceLocator>();
            services.AddSingleton<ICacheService, CacheServiceBase>();
            services.AddSingleton<ITypeLoaderFactory, TypeLoaderFactory>();
            
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        }
    }
}
