using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems
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
