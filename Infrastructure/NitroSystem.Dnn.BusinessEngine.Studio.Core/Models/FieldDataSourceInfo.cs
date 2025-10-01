using System;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.Models
{
    public class FieldDataSourceInfo
    {
        public Guid? ListId { get; set; }
        public FieldDataSourceType Type { get; set; }
        public string ValueField { get; set; }
        public string TextField { get; set; }
        public string VariableName { get; set; }
    }
}
