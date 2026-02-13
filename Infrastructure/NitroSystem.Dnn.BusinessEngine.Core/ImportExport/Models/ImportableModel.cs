using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Models
{
    public class ImportableModel
    {
        public string Version { get; set; }
        public ImportExportScope Scope { get; set; }
        public IEnumerable<ExportedItem> Items { get; set; }
    }
}
