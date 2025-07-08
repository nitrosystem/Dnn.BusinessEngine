using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto
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
