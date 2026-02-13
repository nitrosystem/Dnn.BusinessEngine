using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Models;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Events
{
    public delegate Task ExportProgressHandler(string componentName, bool isSuccess);
    public delegate Task ExportCompletedHandler(ImportExportScope scope, List<ExportedItem> exportedItems);

    public delegate Task ImportProgressHandler(string componentName, bool isSuccess);
    public delegate Task ImportCompletedHandler(ImportContext context);
}
