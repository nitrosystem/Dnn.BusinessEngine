using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ServiceLocator;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ExpressionParser.ExpressionBuilder;

namespace NitroSystem.Dnn.BusinessEngine.Core.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServiceLocator, ServiceLocator>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<ITypeLoaderFactory, TypeLoaderFactory>();

            services.AddScoped<IExpressionService, ExpressionService>();

            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        }
    }
}
