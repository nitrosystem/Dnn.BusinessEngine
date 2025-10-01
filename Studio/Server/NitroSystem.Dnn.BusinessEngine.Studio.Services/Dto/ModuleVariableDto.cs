using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto
{
    public class ModuleVariableDto
    {
        public Guid? AppModelId { get; set; }
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public ModuleVariableScope Scope { get; set; }
        public IEnumerable<PropertyInfo> Properties { get; set; }
    }
}
