using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto
{
   public class BuildModuleResultDto
    {
        public string ModuleTemplate { get; set; }
        public string ModuleFieldsScripts { get; set; }
        public string ModuleActionsScripts { get; set; }
        public bool IsSuccess { get; set; }
        public Exception ErrorException { get; set; }
    }
}
