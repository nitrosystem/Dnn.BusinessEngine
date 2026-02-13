using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Models
{
    public class ExportedItem
    {
        public string ComponentName { get; set; }
        public string ExportedJson { get; set; }

        public ExportedItem(string component, string exportedJson)
        {
            ComponentName = component;
            ExportedJson = exportedJson;
        }
    }
}
