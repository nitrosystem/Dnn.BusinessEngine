using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module
{
    public class ModuleFieldDataSourceSpResult
    {
        public Guid FieldId { get; set; }
        public Guid? ListId { get; set; }
        public int Type { get; set; }
        public string VariableName { get; set; }
        public string TextField { get; set; }
        public string ValueField { get; set; }
    }
}
