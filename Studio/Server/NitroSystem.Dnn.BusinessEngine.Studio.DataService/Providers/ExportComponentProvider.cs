using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
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
    public class ExportComponentProvider : IExportComponentProvider
    {
        private readonly IServiceProvider _sp;

        public ExportComponentProvider(IServiceProvider sp)
        {
            _sp = sp;
        }

        public IEnumerable<ExportComponent> GetComponents()
        {
            return new[]
            {
                new ExportComponent
                {
                    Name = "Scenario",
                    Service = _sp.GetRequiredService<BaseService>(),
                    Priority = 1
                },
                new ExportComponent
                {
                    Name = "Entity",
                    Service = _sp.GetRequiredService<EntityService>(),
                    Priority = 2
                },
                new ExportComponent
                {
                    Name = "AppModel",
                    Service = _sp.GetRequiredService<AppModelService>(),
                    Priority = 3
                },
                new ExportComponent
                {
                    Name = "Service",
                    Service = _sp.GetRequiredService<ServiceFactory>(),
                    Priority = 4
                },
                new ExportComponent
                {
                    Name = "DefinedList",
                    Service = _sp.GetRequiredService<DefinedListService>(),
                    Priority = 5
                },
                new ExportComponent
                {
                    Name = "Module",
                    Service = _sp.GetRequiredService<ModuleService>(),
                    Priority = 6
                },
                new ExportComponent
                {
                    Name = "ModuleLibraryAndResource",
                    Service = _sp.GetRequiredService<ModuleLibraryAndResourceService>(),
                    Priority = 7
                },
                new ExportComponent
                {
                    Name = "ModuleVariable",
                    Service = _sp.GetRequiredService<ModuleVariableService>(),
                    Priority = 8
                },
                new ExportComponent
                {
                    Name = "ModuleField",
                    Service = _sp.GetRequiredService<ModuleFieldService>(),
                    Priority = 9
                },
                new ExportComponent
                {
                    Name = "Dashboard",
                    Service = _sp.GetRequiredService<DashboardService>(),
                    Priority = 10
                },
                new ExportComponent
                {
                    Name = "Action",
                    Service = _sp.GetRequiredService<ActionService>(),
                    Priority = 11
                }
            };
        }
    }
}
