using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildType;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildType.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessEngineStudioEngine(
            this IServiceCollection services)
        {
            //Register Build Module Services
            services.AddScoped<IBuildModuleRunner, BuildModuleRunner>();
            services.AddScoped<BuildModuleEngine>();

            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, InitializeBuildModuleMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, DeleteOldResourcesMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, BuildLayoutMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, MergeResourcesMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildModuleRequest, BuildModuleResponse>, ResourceAggregatorMiddleware>();

            services.AddScoped<InitializeBuildModuleMiddleware>();
            services.AddScoped<DeleteOldResourcesMiddleware>();
            services.AddScoped<BuildLayoutMiddleware>();
            services.AddScoped<MergeResourcesMiddleware>();
            services.AddScoped<ResourceAggregatorMiddleware>();

            services.AddScoped<IBuildLayoutService, BuildLayoutService>();
            services.AddScoped<IMergeResourcesService, MergeResourcesService>();

            //Register Build Type Services
            services.AddScoped<IBuildTypeRunner, BuildTypeRunner>();
            services.AddScoped<BuildTypeEngine>();

            services.AddScoped<IEngineMiddleware<BuildTypeRequest, BuildTypeResponse>, InitializeBuildTypeMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildTypeRequest, BuildTypeResponse>, BuildTypeMiddleware>();

            services.AddScoped<InitializeBuildTypeMiddleware>();
            services.AddScoped<BuildTypeMiddleware>();

            //Register Install Extension Services
            services.AddScoped<IInstallExtensionRunner, InstallExtensionRunner>();
            services.AddScoped<InstallExtensionEngine>();

            services.AddScoped<IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>, ValidateMiddleware>();
            services.AddScoped<IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>, SqlDataProviderMiddleware>();
            services.AddScoped<IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>, ResourcesMiddleware>();

            services.AddScoped<ValidateMiddleware>();
            services.AddScoped<SqlDataProviderMiddleware>();
            services.AddScoped<ResourcesMiddleware>();

            return services;
        }
    }
}
