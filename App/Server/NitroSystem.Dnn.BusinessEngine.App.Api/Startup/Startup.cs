using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ExpressionParser.ExpressionBuilder;
using NitroSystem.Dnn.BusinessEngine.App.Engine.Services;
using NitroSystem.Dnn.BusinessEngine.App.DataServices.ModuleData;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataServices.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.DataServices.Module;
using NitroSystem.Dnn.BusinessEngine.App.DataServices.Action;


namespace NitroSystem.Dnn.BusinessEngine.Api.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IUserDataStore, InMemoryUserDataStore>();

            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IActionService, ActionService>();

            services.AddScoped<IExpressionService, ExpressionService>();

            services.AddScoped<IActionWorker, ActionWorker>();
            services.AddScoped<IActionCondition, ActionCondition>();
        }
    }
}
