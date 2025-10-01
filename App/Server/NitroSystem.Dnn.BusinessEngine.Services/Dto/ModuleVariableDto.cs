using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.App.Services.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Dto
{
    public class ModuleVariableDto
    {
        public Guid Id { get; set; }
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public string DefaultValue { get; set; }
        public ModuleVariableScope Scope { get; set; }
        public string ModelName { get; set; }
        public string ModelTypeRelativePath { get; set; }
        public string ModelTypeFullName { get; set; }
        public string ScenarioName { get; set; }
        public bool IsSystemVariable { get; set; }
        public IEnumerable<PropertyInfo> Properties { get; set; }
    }
}
