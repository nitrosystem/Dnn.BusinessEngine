using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.App.DataService.ModuleData;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.DataService.Module;
using NitroSystem.Dnn.BusinessEngine.App.DataService.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Services;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;


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

            services.AddScoped<IBuildBufferService, BuildBufferService>();
            services.AddScoped<IActionWorker, ActionWorker>();

            services.AddScoped<IEngineMiddleware<ActionRequest, ActionResponse>, ActionConditionMiddleware>();
            services.AddScoped<IEngineMiddleware<ActionRequest, ActionResponse>, ActionWorkerMiddleware>();
            services.AddScoped<IEngineMiddleware<ActionRequest, ActionResponse>, ActionSetResultMiddleware>();
            services.AddScoped<ActionConditionMiddleware>();
            services.AddScoped<ActionWorkerMiddleware>();
            services.AddScoped<ActionSetResultMiddleware>();

            DashboardMappingProfile.Register();
            ModuleMappingProfile.Register();
            ActionMappingProfile.Register();
        }
    }
}
