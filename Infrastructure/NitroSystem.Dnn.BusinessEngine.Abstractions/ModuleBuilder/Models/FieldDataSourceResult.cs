using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Models
{
    public class FieldDataSourceResult
    {
        public string VariableName { get; set; }
        public string ValueField { get; set; }
        public string TextField { get; set; }
        public FieldDataSourceType Type { get; set; }
        public IEnumerable<object> Items { get; set; }
    }
}
