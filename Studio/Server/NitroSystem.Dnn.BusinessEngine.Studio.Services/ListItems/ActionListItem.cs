using System;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ListItems
{
    public class ActionListItem
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? FieldId { get; set; }
        public string ActionType { get; set; }
        public string ActionName { get; set; }
        public string Event { get; set; }
        public ActionExecutionCondition? ParentActionTriggerCondition { get; set; }
    }
}
