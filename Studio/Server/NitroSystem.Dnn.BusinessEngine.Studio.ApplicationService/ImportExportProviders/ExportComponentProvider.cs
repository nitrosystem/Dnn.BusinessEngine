using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.AppModel;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Base;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Entity;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Service;

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
            };
        }
    }
}
