using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems
{
    public class ModuleFieldListItem
    {
        public Guid Id { get; set; }
        public string FieldType { get; set; }
        public string FieldName { get; set; }
        public string FieldText { get; set; }
    }
}
