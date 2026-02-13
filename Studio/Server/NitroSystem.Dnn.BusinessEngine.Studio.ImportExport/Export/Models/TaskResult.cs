using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Models
{
   public class TaskResult
    {
        public bool IsSuccess { get; set; }
        public string Data { get; set; }
        public IDictionary<string, string> Resources { get; set; }
    }
}
