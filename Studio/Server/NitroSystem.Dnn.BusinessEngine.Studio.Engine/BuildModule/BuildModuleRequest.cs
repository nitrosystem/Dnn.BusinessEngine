using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleRequest
    {
        public int UserId { get; set; }
        public string BasePath { get; set; }
        public BuildScope Scope { get; set; }
        public ModuleDto Module { get; set; }
    }
}
