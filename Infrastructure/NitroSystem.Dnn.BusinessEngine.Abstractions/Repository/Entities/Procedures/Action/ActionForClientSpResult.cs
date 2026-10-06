using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Action
{
    public class ActionForClientSpResult
    {
        public Guid Id { get; set; }
        public Guid? FieldId { get; set; }
        public string ActionName { get; set; }
        public string Event { get; set; }
    }
}
