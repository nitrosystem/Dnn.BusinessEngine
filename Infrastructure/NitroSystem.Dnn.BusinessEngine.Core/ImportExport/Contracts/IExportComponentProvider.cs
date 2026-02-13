using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts
{
    public interface IExportComponentProvider
    {
        IEnumerable<ExportComponent> GetComponents();
    }
}
