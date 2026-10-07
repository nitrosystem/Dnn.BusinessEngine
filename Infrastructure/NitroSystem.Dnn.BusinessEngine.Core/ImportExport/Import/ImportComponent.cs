using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import
{
   public class ImportComponent
    {
        public string Name { get; set; }
        public IImportable Service { get; set; }
        public int Priority { get; set; }
    }
}
