using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models
{
    public class ModuleFieldDataSourceResult
    {
        public Guid ActionId { get; set; }
        public string VariableName { get; set; }
        public string ValueField { get; set; }
        public string TextField { get; set; }
        public ModuleFieldDataSourceType Type { get; set; }
        public IEnumerable<object> Items { get; set; }
    }
}
