using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.App.DataService.ModuleData;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.DataService.Module;
using NitroSystem.Dnn.BusinessEngine.App.DataService.Action;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Middlewares;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution;


namespace NitroSystem.Dnn.BusinessEngine.Api.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IUserDataStore, InMemoryUserDataStore>();

            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IActionService, ActionService>();

            services.AddScoped<ActionRunner>();

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

            DashboardMappingProfile.Register();
            ModuleMappingProfile.Register();
            ActionMappingProfile.Register();
        }
    }
}
