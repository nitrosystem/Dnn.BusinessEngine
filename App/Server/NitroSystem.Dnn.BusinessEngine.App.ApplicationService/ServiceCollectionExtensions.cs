using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.ApplicationService.Action;
using NitroSystem.Dnn.BusinessEngine.App.ApplicationService.Dashboard;
using NitroSystem.Dnn.BusinessEngine.App.ApplicationService.Module;
using NitroSystem.Dnn.BusinessEngine.App.ApplicationService.ModuleData;

namespace NitroSystem.Dnn.BusinessEngine.App.ApplicationService
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessEngineAppApplication(
            this IServiceCollection services)
        {
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IModuleVariableService, ModuleVariableService>();
            services.AddScoped<IModuleFieldService, ModuleFieldService>();
            services.AddScoped<IActionService, ActionService>();
            
            services.AddSingleton<IUserDataStore, InMemoryUserDataStore>();

            DashboardMappingProfile.Register();
            ModuleMappingProfile.Register();
            ActionMappingProfile.Register();

            return services;
        }
    }
}
