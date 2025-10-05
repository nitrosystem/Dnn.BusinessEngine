using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Dto
{
   public class ModuleResourceDto
    {
        public ResourceType ResourceType { get; set; }
        public ResourceContentType ResourceContentType { get; set; }
        public string ResourcePath { get; set; }
        public string EntryType { get; set; }
        public bool IsContent { get; set; }
        public string Content { get; set; }
    }
}
