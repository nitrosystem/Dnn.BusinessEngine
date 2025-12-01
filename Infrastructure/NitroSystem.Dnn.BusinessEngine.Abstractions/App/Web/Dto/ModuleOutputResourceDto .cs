using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto
{
    public class ModuleOutputResourceDto 
    {
        public ResourceContentType ResourceContentType { get; set; }
        public string ResourcePath { get; set; }
        public bool IsSystemResource { get; set; }
        public int LoadOrder { get; set; }
    }
}