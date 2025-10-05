using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Dto
{
    public class ModuleFieldTypeDto
    {
        public Guid Id { get; set; }
        public string FieldType { get; set; }
        public string FieldName { get; set; }
        public string Template { get; set; }
        public string TemplatePath { get; set; }
    }
}
