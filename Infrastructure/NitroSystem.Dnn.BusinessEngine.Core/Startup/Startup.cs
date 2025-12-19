using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.ServiceLocator;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ExpressionBuilder;
using NitroSystem.Dnn.BusinessEngine.Core.PushingServer;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.PushingServer;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;

namespace NitroSystem.Dnn.BusinessEngine.Core.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServiceLocator, ServiceLocator>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<ITypeLoaderFactory, TypeLoaderFactory>();

            services.AddSingleton<GeneratedModelRegistry>();

            services.AddSingleton<LockService>();

            services.AddScoped<IEngineRunner, EngineRunner>();
            
            services.AddScoped<IExpressionService, ExpressionService>();

            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            services.AddSingleton<WorkflowManager>();
            services.AddSingleton<ResourceProfiler>();

            services.AddSingleton<INotificationServerHost, NotificationServerHost>();
            services.AddSingleton<IWebSocketManager, WebSocketManager>();
            services.AddSingleton<INotificationServer, NotificationServerProxy>();

            services.AddSingleton<BackgroundFramework>(sp =>
            {
                // حتما فقط یکبار ساخته می‌شود
                return new BackgroundFramework(
                    serviceProvider: sp,
                    maxDegreeOfParallelism: 3,          // یا هر عدد دلخواه
                    memoryThresholdBytes: 500 * 1024 * 1024,
                    webSocketChannel: null              // کانال پیشفرض می‌تواند null باشد
                );
            });
        }
    }
}
