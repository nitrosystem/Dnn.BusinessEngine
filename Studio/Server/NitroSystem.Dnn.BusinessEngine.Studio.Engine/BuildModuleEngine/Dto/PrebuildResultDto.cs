using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Dto
{
   public class PrebuildResultDto
    {
        public string TemplateDirectoryPath { get; set; }
        public bool IsDeletedOldResources { get; set; }
        public bool IsDeletedOldFiles { get; set; }
        public bool IsReadyToBuild { get; set; }
        public Exception ExceptionError { get; set; }
    }
}
