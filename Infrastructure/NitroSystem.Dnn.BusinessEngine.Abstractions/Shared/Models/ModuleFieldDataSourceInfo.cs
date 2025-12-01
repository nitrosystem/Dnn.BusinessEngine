using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models
{
    public class ModuleFieldDataSourceInfo
    {
        public string ListName { get; set; }
        public string VariableName { get; set; }
        public string ValueField { get; set; }
        public string TextField { get; set; }
        public ModuleFieldDataSourceType Type { get; set; }
    }
}
