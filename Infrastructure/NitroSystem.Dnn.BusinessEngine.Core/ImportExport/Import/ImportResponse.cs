using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import
{
    public class ImportResponse
    {
        public object Result { get; set; }
        public bool IsSuccess { get; set; }
        public List<string> Warnings { get; set; }
    }
}
