using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Middlewares;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessEngineAppEngine(
            this IServiceCollection services)
        {
            services.AddTransient<ActionRunner>();
            services.AddTransient<ActionExecutionEngine>();

            services.AddScoped<IEngineMiddleware<ActionRequest, ActionResponse>, ActionConditionMiddleware>();
            services.AddScoped<IEngineMiddleware<ActionRequest, ActionResponse>, BeforeExecuteActionMiddleware>();
            services.AddScoped<IEngineMiddleware<ActionRequest, ActionResponse>, ActionSetParamsMiddleware>();
            services.AddScoped<IEngineMiddleware<ActionRequest, ActionResponse>, ActionWorkerMiddleware>();
            services.AddScoped<IEngineMiddleware<ActionRequest, ActionResponse>, ActionSetResultsMiddleware>();

            services.AddScoped<ActionConditionMiddleware>();
            services.AddScoped<BeforeExecuteActionMiddleware>();
            services.AddScoped<ActionSetParamsMiddleware>();
            services.AddScoped<ActionWorkerMiddleware>();
            services.AddScoped<ActionSetResultsMiddleware>();

            return services;
        }
    }
}
