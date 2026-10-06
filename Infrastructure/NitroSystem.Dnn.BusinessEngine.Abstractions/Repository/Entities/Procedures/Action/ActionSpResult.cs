using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Action
{
    public class ActionSpResult
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? FieldId { get; set; }
        public string ActionType { get; set; }
        public string ActionName { get; set; }
        public string Event { get; set; }
        public int? ParentActionTriggerCondition { get; set; }
        public int ExecuteOrder { get; set; }
        public string ActionConditionsDsl { get; set; }
        public string BeforeExecuteActionDsl { get; set; }
        public string ActionResultsDsl { get; set; }
        public string AuthorizationRunAction { get; set; }
        public string Settings { get; set; }
        public bool IsRedirectable { get; set; }
    }
}
