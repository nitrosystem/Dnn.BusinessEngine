using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Models
{
    public class FieldDataSourceInfo
    {
        public Guid? ListId { get; set; }
        public string ValueField { get; set; }
        public string TextField { get; set; }
        public string VariableName { get; set; }
        public FieldDataSourceType Type { get; set; }
    }
}
