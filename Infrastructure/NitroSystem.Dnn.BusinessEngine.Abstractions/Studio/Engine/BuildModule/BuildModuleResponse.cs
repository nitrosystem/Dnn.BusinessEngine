using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule
{
    public class BuildModuleResponse
    {
        public IEnumerable<ModuleResourceDto> FinalizedResources { get; set; }

        public bool IsSuccess { get; set; }
    }
}
