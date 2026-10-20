using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.Services;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio
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
                    Name = "DataSourceService",
                    Service = _sp.GetRequiredService<DataSourceService>(),
                    Priority = 101
                },
                new ExportComponent
                {
                    Name = "DataRowService",
                    Service = _sp.GetRequiredService<DataRowService>(),
                    Priority = 102
                },
                new ExportComponent
                {
                    Name = "BindEntityService",
                    Service = _sp.GetRequiredService<BindEntityService>(),
                    Priority = 103
                },
                new ExportComponent
                {
                    Name = "SubmitEntityService",
                    Service = _sp.GetRequiredService<SubmitEntityService>(),
                    Priority = 104
                },
                new ExportComponent
                {
                    Name = "DeleteEntityRowService",
                    Service = _sp.GetRequiredService<DeleteEntityRowService>(),
                    Priority = 105
                },
            };
        }
    }
}
