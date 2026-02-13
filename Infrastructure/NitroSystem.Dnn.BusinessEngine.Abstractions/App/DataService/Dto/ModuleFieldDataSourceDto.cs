using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto
{
   public class ModuleFieldDataSourceDto
    {
        public Guid? ListId { get; set; }
        public string VariableName { get; set; }
        public string ValueField { get; set; }
        public string TextField { get; set; }
        public ModuleFieldDataSourceType Type { get; set; }
        public IEnumerable<object> Items { get; set; }
    }
}
