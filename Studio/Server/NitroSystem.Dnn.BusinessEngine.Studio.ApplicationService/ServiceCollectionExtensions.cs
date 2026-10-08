using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Action;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.AppModel;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.BackgroundJobs;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Base;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Entity;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Extension;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.ImportExportProviders;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Module;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Provider;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Service;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Template;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Providers;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessEngineStudioApplication(
            this IServiceCollection services)
        {
            services.AddScoped<IExtensionService, ExtensionService>();
            services.AddScoped<IProviderService, ProviderService>();
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<IEntityService, EntityService>();
            services.AddScoped<IAppModelService, AppModelService>();
            services.AddScoped<IServiceFactory, ServiceFactory>();
            services.AddScoped<IDefinedListService, DefinedListService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IModuleLibraryAndResourceService, ModuleLibraryAndResourceService>();
            services.AddScoped<IModuleVariableService, ModuleVariableService>();
            services.AddScoped<IModuleFieldService, ModuleFieldService>();
            services.AddScoped<IActionService, ActionService>();

            services.AddScoped<BuildModuleJob>();

            ActionMappingProfile.Register();
            AppModelMappingProfile.Register();
            BaseMappingProfile.Register();
            DashboardMappingProfile.Register();
            EntityMappingProfile.Register();
            ExtensionMappingProfile.Register();
            ServiceMappingProfile.Register();
            ProviderMappingProfile.Register();
            ModuleMappingProfile.Register();
            TemplateMappingProfile.Register();

            services.AddTransient<IExportComponentProvider, ExportComponentProvider>();
            services.AddTransient<IImportComponentProvider, ImportComponentProvider>();

            services.AddTransient<Func<IEnumerable<ExportComponent>>>(sp =>
            {
                var providers = sp.GetServices<IExportComponentProvider>();

                return () =>
                    providers
                        .SelectMany(p => p.GetComponents())
                        .OrderBy(c => c.Priority);
            });

            services.AddTransient<Func<ImportExportScope, IEnumerable<ImportComponent>>>(sp =>
            {
                var providers = sp.GetServices<IImportComponentProvider>();

                return scope =>
                    providers
                        .SelectMany(p => p.GetComponents(scope))
                        .OrderBy(c => c.Priority);
            });

            return services;
        }
    }
}
