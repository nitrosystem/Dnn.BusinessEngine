using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Action;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Template;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Service;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.AppModel;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Entity;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Base;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Extension;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilder;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Api.BackgroundJobs;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Providers;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Middlewares;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<IEntityService, EntityService>();
            services.AddScoped<IAppModelService, AppModelService>();
            services.AddScoped<IServiceFactory, ServiceFactory>();

            services.AddScoped<IExtensionService, ExtensionService>();
            services.AddScoped<IDefinedListService, DefinedListService>();

            services.AddScoped<IDashboardService, DashboardService>();

            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IModuleTemplateService, ModuleTemplateService>();
            services.AddScoped<IModuleLibraryAndResourceService, ModuleLibraryAndResourceService>();
            services.AddScoped<IModuleVariableService, ModuleVariableService>();
            services.AddScoped<IModuleFieldService, ModuleFieldService>();

            services.AddScoped<IActionService, ActionService>();

            services.AddScoped<ITemplateService, TemplateService>();

            services.AddScoped<BaseService>();
            services.AddScoped<EntityService>();
            services.AddScoped<AppModelService>();
            services.AddScoped<ServiceFactory>();
            services.AddScoped<DefinedListService>();
            services.AddScoped<DashboardService>();
            services.AddScoped<ModuleService>();
            services.AddScoped<ModuleLibraryAndResourceService>();
            services.AddScoped<ModuleVariableService>();
            services.AddScoped<ModuleFieldService>();
            services.AddScoped<ActionService>();

            services.AddScoped<IBuildLayoutService, BuildLayoutService>();
            services.AddScoped<IMergeResourcesService, MergeResourcesService>();
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
            services.AddScoped<BuildModuleRunner>();

            services.AddScoped<BuildModuleJob>();

            services.AddScoped<IEngineMiddleware<BuildTypeRequest, BuildTypeResponse>, InitializeBuildTypeMiddleware>();
            services.AddScoped<IEngineMiddleware<BuildTypeRequest, BuildTypeResponse>, BuildTypeMiddleware>();
            services.AddScoped<InitializeBuildTypeMiddleware>();
            services.AddScoped<BuildTypeMiddleware>();
            services.AddScoped<BuildTypeRunner>();

            services.AddScoped<IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>, ValidateMiddleware>();
            services.AddScoped<IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>, SqlDataProviderMiddleware>();
            services.AddScoped<IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>, ResourcesMiddleware>();
            services.AddScoped<ValidateMiddleware>();
            services.AddScoped<SqlDataProviderMiddleware>();
            services.AddScoped<ResourcesMiddleware>();
            services.AddScoped<InstallExtensionRunner>();

            BaseMappingProfile.Register();
            EntityMappingProfile.Register();
            AppModelMappingProfile.Register();
            ServiceMappingProfile.Register();
            ModuleMappingProfile.Register();
            DashboardMappingProfile.Register();
            ActionMappingProfile.Register();
            TemplateMappingProfile.Register();
            ExtensionMappingProfile.Register();

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


            //services.AddTransient<Func<ImportExportScope, IEnumerable<ExportComponent>>>(sp =>
            //{
            //    return scope =>
            //    {
            //        return scope switch
            //        {
            //            ImportExportScope.ScenarioFullComponents =>
            //                new[]
            //                {
            //                    new ExportComponent
            //                    {
            //                        Name = "Scenario",
            //                        Service = sp.GetRequiredService<BaseService>()
            //                    },
            //                    new ExportComponent
            //                    {
            //                        Name = "Entity",
            //                        Service = sp.GetRequiredService<EntityService>()
            //                    },
            //                    new ExportComponent
            //                    {
            //                        Name = "AppModel",
            //                        Service = sp.GetRequiredService<AppModelService>()
            //                    },
            //                    new ExportComponent
            //                    {
            //                        Name = "Service",
            //                        Service = sp.GetRequiredService<ServiceFactory>()
            //                    },
            //                    new ExportComponent
            //                    {
            //                        Name = "Module",
            //                        Service = sp.GetRequiredService<ModuleService>()
            //                    },
            //                    new ExportComponent
            //                    {
            //                        Name = "Dashboard",
            //                        Service = sp.GetRequiredService<DashboardService>()
            //                    }
            //                },
            //            ImportExportScope.Module =>
            //                new[]
            //                {
            //                    new ExportComponent
            //                    {
            //                        Name = "Module",
            //                        Service = sp.GetRequiredService<ModuleService>()
            //                    },
            //                    new ExportComponent
            //                    {
            //                        Name = "ModuleLibraryAndResource",
            //                        Service = sp.GetRequiredService<ModuleLibraryAndResourceService>()
            //                    },
            //                    new ExportComponent
            //                    {
            //                        Name = "ModuleVariable",
            //                        Service = sp.GetRequiredService<ModuleVariableService>()
            //                    },
            //                    new ExportComponent
            //                    {
            //                        Name = "ModuleField",
            //                        Service = sp.GetRequiredService<ModuleFieldService>()
            //                    },
            //                    new ExportComponent
            //                    {
            //                        Name = "Action",
            //                        Service = sp.GetRequiredService<ActionService>()
            //                    }
            //                },
            //            _ => null
            //        };
            //    };
            //});

            //services.AddTransient<Func<ImportExportScope, IEnumerable<ImportComponent>>>(sp =>
            //{
            //    return scope =>
            //    {
            //        return scope switch
            //        {
            //            ImportExportScope.ScenarioFullComponents =>
            //                new[]
            //                {
            //                    //new ExportComponent
            //                    //{
            //                    //    Name = "Scenario",
            //                    //    Service = sp.GetRequiredService<BaseService>()
            //                    //},
            //                    //new ExportComponent
            //                    //{
            //                    //    Name = "Entity",
            //                    //    Service = sp.GetRequiredService<EntityService>()
            //                    //},
            //                    //new ExportComponent
            //                    //{
            //                    //    Name = "AppModel",
            //                    //    Service = sp.GetRequiredService<AppModelService>()
            //                    //},
            //                    //new ExportComponent
            //                    //{
            //                    //    Name = "Service",
            //                    //    Service = sp.GetRequiredService<ServiceFactory>()
            //                    //},
            //                    new ImportComponent
            //                    {
            //                        Name = "Module",
            //                        Service = sp.GetRequiredService<ModuleService>()
            //                    },
            //                    //new ExportComponent
            //                    //{
            //                    //    Name = "Dashboard",
            //                    //    Service = sp.GetRequiredService<DashboardService>()
            //                    //}
            //                },
            //            _ => null
            //        };
            //    };
            //});

            //services.AddTransient<Func<ImportExportScope, IEnumerable<ImportComponent>>>(sp =>
            //{
            //    return scope =>
            //    {
            //        if (scope != ImportExportScope.Module)
            //            return Enumerable.Empty<ImportComponent>();

            //        return new[]
            //        {
            //            new ImportComponent
            //            {
            //                Name = "Module",
            //                Service = sp.GetRequiredService<ModuleService>()
            //            },
            //            new ImportComponent
            //            {
            //                Name = "ModuleLibraryAndResource",
            //                Service = sp.GetRequiredService<ModuleLibraryAndResourceService>()
            //            },
            //            new ImportComponent
            //            {
            //                Name = "ModuleVariable",
            //                Service = sp.GetRequiredService<ModuleVariableService>()
            //            },
            //            new ImportComponent
            //            {
            //                Name = "ModuleField",
            //                Service = sp.GetRequiredService<ModuleFieldService>()
            //            },
            //            new ImportComponent
            //            {
            //                Name = "Action",
            //                Service = sp.GetRequiredService<ActionService>()
            //            }
            //        };
            //    };
            //});
        }
    }
}
