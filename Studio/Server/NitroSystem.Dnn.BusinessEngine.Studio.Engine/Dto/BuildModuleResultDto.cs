using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto
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
