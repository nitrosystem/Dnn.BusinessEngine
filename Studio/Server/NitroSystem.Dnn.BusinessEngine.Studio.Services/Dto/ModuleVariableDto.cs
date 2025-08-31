using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
   public class ModuleVariableDto
    {
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public ModuleVariableScope Scope { get; set; }
        public IEnumerable<PropertyInfo> Properties { get; set; }
    }
}
