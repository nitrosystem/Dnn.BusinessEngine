using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Extension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Library;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Template;
using NitroSystem.Dnn.BusinessEngine.Repository.Action;
using NitroSystem.Dnn.BusinessEngine.Repository.AppModel;
using NitroSystem.Dnn.BusinessEngine.Repository.Base;
using NitroSystem.Dnn.BusinessEngine.Repository.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Repository.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Repository.Entity;
using NitroSystem.Dnn.BusinessEngine.Repository.Extension;
using NitroSystem.Dnn.BusinessEngine.Repository.Module;
using NitroSystem.Dnn.BusinessEngine.Repository.Library;
using NitroSystem.Dnn.BusinessEngine.Repository.Service;
using NitroSystem.Dnn.BusinessEngine.Repository.Template;

namespace NitroSystem.Dnn.BusinessEngine.Repository
{
    public static class RepositoryCollectionExtensions
    {
        public static IServiceCollection AddBusinessEngineRepository(
            this IServiceCollection services)
        {
            services.AddScoped<IExtensionRepository, ExtensionRepository>();
            services.AddScoped<IProviderRepository, ProviderRepository>();
            services.AddScoped<ITemplateRepository, TemplateRepository>();
            services.AddScoped<ITemplateThemeRepository, TemplateThemeRepository>();
            services.AddScoped<ILibraryRepository, LibraryRepository>();
            services.AddScoped<ILibraryResourceRepository, LibraryResourceRepository>();
            services.AddScoped<IScenarioRepository, ScenarioRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IEntityRepository, EntityRepository>();
            services.AddScoped<IEntityColumnRepository, EntityColumnRepository>();
            services.AddScoped<IAppModelRepository, AppModelRepository>();
            services.AddScoped<IAppModelPropertyRepository, AppModelPropertyRepository>();
            services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IServiceParamRepository, ServiceParamRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IDashboardPageRepository, DashboardPageRepository>();
            services.AddScoped<IDashboardPageModuleRepository, DashboardPageModuleRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<IModuleOutputResourceRepository, ModuleOutputResourceRepository>();
            services.AddScoped<IModuleVariableRepository, ModuleVariableRepository>();
            services.AddScoped<IModuleFieldTypeRepository, ModuleFieldTypeRepository>();
            services.AddScoped<IModuleFieldTypeTemplateRepository, ModuleFieldTypeTemplateRepository>();
            services.AddScoped<IModuleFieldTypeThemeRepository, ModuleFieldTypeThemeRepository>();
            services.AddScoped<IModuleFieldRepository, ModuleFieldRepository>();
            services.AddScoped<IModuleFieldDataSourceRepository, ModuleFieldDataSourceRepository>();
            services.AddScoped<IModuleFieldSettingRepository, ModuleFieldSettingRepository>();
            services.AddScoped<IModuleCustomLibraryRepository, ModuleCustomLibraryRepository>();
            services.AddScoped<IModuleCustomResourceRepository, ModuleCustomResourceRepository>();
            services.AddScoped<IModuleEventTypeRepository, ModuleEventTypeRepository>();
            services.AddScoped<IActionTypeRepository, ActionTypeRepository>();
            services.AddScoped<IActionRepository, ActionRepository>();
            services.AddScoped<IActionParamRepository, ActionParamRepository>();
            services.AddScoped<IDefinedListRepository, DefinedListRepository>();
            services.AddScoped<IDefinedListItemRepository, DefinedListItemRepository>();

            return services;
        }
    }
}
