using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule
{
    public class BuildModuleRequest
    {
        public int UserId { get; set; }
        public string BasePath { get; set; }
        public ModuleDto Module { get; set; }
    }
}
