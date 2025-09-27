using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Models.Shared
{
    public class FieldDataSourceResult
    {
        public FieldDataSourceType Type { get; set; }
        public string VariableName { get; set; }
        public string ValueField { get; set; }
        public string TextField { get; set; }
        public IEnumerable<object> Items { get; set; }
    }
}
