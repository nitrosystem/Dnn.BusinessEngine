using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export
{
    public class ExportRequest
    {
        public Guid Id { get; set; }
        public string ExportName { get; set; }
        public string Channel { get; set; }
        public string Version { get; set; }
        public ImportExportScope ExportScope { get; set; }
        public Dictionary<string, object> Params { get; set; }
    }
}
