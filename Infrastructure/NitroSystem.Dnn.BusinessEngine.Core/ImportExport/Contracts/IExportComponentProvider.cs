using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts
{
    public interface IExportComponentProvider
    {
        IEnumerable<ExportComponent> GetComponents();
    }
}
