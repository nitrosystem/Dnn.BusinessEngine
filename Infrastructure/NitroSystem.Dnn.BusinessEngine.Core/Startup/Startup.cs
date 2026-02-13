using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.ServiceLocator;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ExpressionBuilder;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.SseNotifier;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS;

namespace NitroSystem.Dnn.BusinessEngine.Core.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServiceLocator, ServiceLocator>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<ITypeLoaderFactory, TypeLoaderFactory>();

            services.AddSingleton<IDiagnosticStore, DiagnosticStore>();

            services.AddSingleton<GeneratedModelRegistry>();

            services.AddSingleton<LockService>();

            services.AddScoped<IEngineRunner, EngineRunner>();

            services.AddSingleton<ISmsProviderResolver, SmsProviderResolver>();
            services.AddScoped<ISmsService, SmsService>();

            services.AddScoped<IExpressionService, ExpressionService>();

            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            services.AddSingleton<ISseNotifier, SseNotifier.SseNotifier>();

            services.AddSingleton<BackgroundJobWorker>(sp =>
                new BackgroundJobWorker(
                    sp.GetRequiredService<IServiceScopeFactory>(),
                    maxDegreeOfParallelism: 3
                )
            );
        }
    }
}
