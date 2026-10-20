using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.Services;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio
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
                        Name = "DataSourceService",
                        Service = _sp.GetRequiredService<DataSourceService>(),
                        Priority = 101
                    },
                    new ImportComponent
                    {
                        Name = "DataRowService",
                        Service = _sp.GetRequiredService<DataRowService>(),
                        Priority = 102
                    },
                    new ImportComponent
                    {
                        Name = "BindEntityService",
                        Service = _sp.GetRequiredService<BindEntityService>(),
                        Priority = 103
                    },
                    new ImportComponent
                    {
                        Name = "SubmitEntityService",
                        Service = _sp.GetRequiredService<SubmitEntityService>(),
                        Priority = 104
                    },
                    new ImportComponent
                    {
                        Name = "DeleteEntityRowService",
                        Service = _sp.GetRequiredService<DeleteEntityRowService>(),
                        Priority = 105
                    }
                },
                _ => Enumerable.Empty<ImportComponent>()
            };
        }
    }
}
