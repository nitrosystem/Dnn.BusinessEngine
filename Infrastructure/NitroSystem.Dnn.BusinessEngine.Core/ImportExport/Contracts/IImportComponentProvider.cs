using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts
{
    public interface IImportComponentProvider
    {
        IEnumerable<ImportComponent> GetComponents(ImportExportScope scope);
    }
}
