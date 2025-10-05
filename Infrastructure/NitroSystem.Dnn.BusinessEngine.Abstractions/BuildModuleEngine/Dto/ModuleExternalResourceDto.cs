using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Dto
{
  public  class ModuleExternalResourceDto
    {
        public ExternalResourceType ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public string LoadOrder { get; set; }
    }
}
