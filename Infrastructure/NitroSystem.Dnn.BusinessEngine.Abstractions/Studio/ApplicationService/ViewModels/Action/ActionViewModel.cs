using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Action
{
    public class ActionViewModel
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? FieldId { get; set; }
        public string ActionType { get; set; }
        public string ActionName { get; set; }
        public string Event { get; set; }
        public string ActionConditionsDsl { get; set; }
        public string BeforeExecuteActionDsl { get; set; }
        public string ActionResultsDsl { get; set; }
        public string ActionTypeIcon { get; set; }
        public string ActionTypeTitle { get; set; }
        public string FieldType { get; set; }
        public string FieldName { get; set; }
        public int FieldViewOrder { get; set; }
        public string ServiceType{ get; set; }
        public string ServiceName { get; set; }
        public int ExecuteOrder { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserDisplayName { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public string LastModifiedByUserDisplayName { get; set; }
        public ActionExecutionCondition? ParentActionTriggerCondition { get; set; }
        public IEnumerable<string> AuthorizationRunAction { get; set; }
        public IEnumerable<ActionParamViewModel> Params { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}