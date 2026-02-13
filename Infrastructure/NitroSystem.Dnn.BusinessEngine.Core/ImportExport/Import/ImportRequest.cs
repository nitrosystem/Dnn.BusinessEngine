using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Dto
{
    public class ImportRequest
    {
        public Guid Id { get; set; }
        public string Channel { get; set; }
        public string ExportedFile { get; set; }
        public ImportExportScope ImportScope { get; set; }
        public Dictionary<string, object> Params { get; set; }
    }
}
