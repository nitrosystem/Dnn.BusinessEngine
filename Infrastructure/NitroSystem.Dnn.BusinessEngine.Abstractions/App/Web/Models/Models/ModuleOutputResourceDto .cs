using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Models
{
    public class ModuleOutputResourceDto 
    {
        public ModuleResourceContentType ResourceContentType { get; set; }
        public string ResourcePath { get; set; }
        public bool IsSystemResource { get; set; }
        public int LoadOrder { get; set; }
    }
}