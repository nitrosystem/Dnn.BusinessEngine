using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob;
using NitroSystem.Dnn.BusinessEngine.Core.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Functions;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Email;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Core.SseNotifier;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.ServiceLocator;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Core
{
    public static class RepositoryCollectionExtensions
    {
        public static IServiceCollection AddBusinessEngineCore(
            this IServiceCollection services)
        {
            services.AddSingleton<IServiceLocator, ServiceLocator>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<ITypeLoaderFactory, TypeLoaderFactory>();

            services.AddSingleton<IDiagnosticStore, DiagnosticStore>();

            services.AddSingleton<GeneratedModelRegistry>();

            services.AddSingleton<LockService>();

            services.AddScoped<IEngineRunner, EngineRunner>();

            services.AddScoped<IExpressionService, ExpressionService>();

            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            services.AddSingleton<IEmailProviderResolver, EmailProviderResolver>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddSingleton<ISmsProviderResolver, SmsProviderResolver>();
            services.AddScoped<ISmsService, SmsService>();

            services.AddSingleton<ISseNotifier, SseNotifier.SseNotifier>();

            services.AddSingleton<BackgroundJobWorker>(sp =>
                new BackgroundJobWorker(
                    sp.GetRequiredService<IServiceScopeFactory>(),
                    maxDegreeOfParallelism: 3
                )
            );

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && IsRelevantAssembly(a));
            ExpressionFunctionScanner.ScanAndRegister(assemblies);

            return services;
        }

        private static bool IsRelevantAssembly(Assembly a)
        {
            try
            {
                return a.GetReferencedAssemblies()
                    .Any(r => r.Name == typeof(ExpressionFunctionAttribute).Assembly.GetName().Name);
            }
            catch
            {
                return false;
            }
        }
    }
}
