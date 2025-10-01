using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ExpressionParser.ExpressionBuilder;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Services;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Services;
using NitroSystem.Dnn.BusinessEngine.App.Framework.Services;
using NitroSystem.Dnn.BusinessEngine.App.Services.ModuleData;


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
