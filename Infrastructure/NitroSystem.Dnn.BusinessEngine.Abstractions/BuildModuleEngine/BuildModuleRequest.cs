using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine
{
    public class BuildModuleRequest
    {
        public Guid? ModuleId { get { return Module?.Id; } }
        public string ModuleName { get { return Module?.ModuleName; } }
        public string BasePath { get; set; }
        public string BuildPath { get; set; }
        public BuildScope Scope { get; set; }
        public ModuleDto Module { get; set; }
    }
}
