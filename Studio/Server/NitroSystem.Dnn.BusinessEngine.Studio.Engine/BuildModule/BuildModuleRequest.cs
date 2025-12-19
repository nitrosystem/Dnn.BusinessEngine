using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleRequest
    {
        public int UserId { get; set; }
        public Guid? ModuleId { get { return Module?.Id; } }
        public string ModuleName { get { return Module?.ModuleName; } }
        public string BasePath { get; set; }
        public BuildScope Scope { get; set; }
        public ModuleDto Module { get; set; }
    }
}
