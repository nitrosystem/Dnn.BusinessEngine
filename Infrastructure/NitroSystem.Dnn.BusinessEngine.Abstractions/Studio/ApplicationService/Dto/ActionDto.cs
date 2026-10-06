using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Dto
{
    public class ActionDto
    {
        public Guid ModuleId { get; set; }
        public Guid? ActionId { get; set; }
        public Guid? FieldId { get; set; }
        public string ActionName { get; set; }
        public string FieldType { get; set; }
    }
}
