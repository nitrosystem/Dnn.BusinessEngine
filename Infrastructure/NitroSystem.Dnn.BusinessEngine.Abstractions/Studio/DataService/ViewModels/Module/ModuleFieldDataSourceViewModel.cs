using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module
{
    public class ModuleFieldDataSourceViewModel
    {
        public Guid Id { get; set; }
        public Guid FieldId { get; set; }
        public Guid? ListId { get; set; }
        public string VariableName { get; set; }
        public string TextField { get; set; }
        public string ValueField { get; set; }
        public ModuleFieldDataSourceType Type { get; set; }
    }
}
