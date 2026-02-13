using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts
{
    public interface IImportComponentProvider
    {
        IEnumerable<ImportComponent> GetComponents(ImportExportScope scope);
    }
}
