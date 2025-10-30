using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Models
{
    public class FieldDataSourceInfo
    {
        public string ListName { get; set; }
        public string VariableName { get; set; }
        public string ValueField { get; set; }
        public string TextField { get; set; }
        public FieldDataSourceType Type { get; set; }
    }
}
