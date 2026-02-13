using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Action;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.AppModel;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Base;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Entity;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module;
using NitroSystem.Dnn.BusinessEngine.Studio.DataService.Service;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Providers
{
    public class ImportComponentProvider : IImportComponentProvider
    {
        private readonly IServiceProvider _sp;

        public ImportComponentProvider(IServiceProvider sp)
        {
            _sp = sp;
        }

        public IEnumerable<ImportComponent> GetComponents(ImportExportScope scope)
        {
            return scope switch
            {
                ImportExportScope.ScenarioFullComponents => new[]
                {
                    new ImportComponent
                    {
                        Name = "Scenario",
                        Service = _sp.GetRequiredService<BaseService>(),
                        Priority = 1
                    },
                    new ImportComponent
                    {
                        Name = "Entity",
                        Service = _sp.GetRequiredService<EntityService>(),
                        Priority = 2
                    },
                    new ImportComponent
                    {
                        Name = "AppModel",
                        Service = _sp.GetRequiredService<AppModelService>(),
                        Priority = 3
                    },
                    new ImportComponent
                    {
                        Name = "Service",
                        Service = _sp.GetRequiredService<ServiceFactory>(),
                        Priority = 4
                    },
                    new ImportComponent
                    {
                        Name = "DefinedList",
                        Service = _sp.GetRequiredService<DefinedListService>(),
                        Priority = 5
                    },
                    new ImportComponent
                    {
                        Name = "Module",
                        Service = _sp.GetRequiredService<ModuleService>(),
                        Priority = 6
                    },
                    new ImportComponent
                    {
                        Name = "ModuleLibraryAndResource",
                        Service = _sp.GetRequiredService<ModuleLibraryAndResourceService>(),
                        Priority = 7
                    },
                    new ImportComponent
                    {
                        Name = "ModuleVariable",
                        Service = _sp.GetRequiredService<ModuleVariableService>(),
                        Priority = 8
                    },
                    new ImportComponent
                    {
                        Name = "ModuleField",
                        Service = _sp.GetRequiredService<ModuleFieldService>(),
                        Priority = 9
                    },
                    new ImportComponent
                    {
                        Name = "Dashboard",
                        Service = _sp.GetRequiredService<DashboardService>(),
                        Priority = 10
                    },
                    new ImportComponent
                    {
                        Name = "Action",
                        Service = _sp.GetRequiredService<ActionService>(),
                        Priority = 11
                    }
                },
                _ => Enumerable.Empty<ImportComponent>()
            };
        }
    }
}
