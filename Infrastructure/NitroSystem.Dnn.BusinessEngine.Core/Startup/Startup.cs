using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ServiceLocator;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ExpressionParser.ExpressionBuilder;
using NitroSystem.Dnn.BusinessEngine.Core.PushingServer.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.PushingServer;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow;

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

            services.AddSingleton<WorkflowEventManager>();
            services.AddSingleton<ResourceProfiler>();

            services.AddSingleton<INotificationServerHost, NotificationServerHost>();
            services.AddSingleton<NotificationServer>();
            var serviceProvider = services.BuildServiceProvider();
            var wsHost = serviceProvider.GetRequiredService<INotificationServerHost>();
            wsHost.Start();

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
