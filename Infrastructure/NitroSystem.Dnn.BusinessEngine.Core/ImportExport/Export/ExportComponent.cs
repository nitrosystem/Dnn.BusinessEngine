using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export
{
    public class ExportComponent
    {
        public string Name { get; set; }
        public IExportable Service { get; set; }
        public int Priority { get; set; }
    }
}
