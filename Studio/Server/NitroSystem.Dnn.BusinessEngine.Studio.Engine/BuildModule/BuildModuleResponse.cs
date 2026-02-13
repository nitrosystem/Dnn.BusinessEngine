using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class BuildModuleResponse
    {
        public IEnumerable<ModuleResourceDto> FinalizedResources { get; set; }

        public bool IsSuccess { get; set; }

        public Exception Exception { get; set; }
    }
}
