using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module
{
    public class ModuleFieldSettingSpResult
    {
        public Guid Id { get; set; }
        public Guid FieldId { get; set; }
        public Guid ModuleId { get; set; }
        public string FieldName { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
    }
}
