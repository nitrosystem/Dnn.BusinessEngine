using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems
{
    public class ModuleVariableListItem
    {
        public Guid? AppModelId { get; set; }
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public ModuleVariableScope Scope { get; set; }
        public IEnumerable<PropertyInfo> Properties { get; set; }
    }
}
