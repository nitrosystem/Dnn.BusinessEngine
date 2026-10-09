using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Base;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Entity;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.ImportExportProviders
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
                    }
                },
                _ => Enumerable.Empty<ImportComponent>()
            };
        }
    }
}
