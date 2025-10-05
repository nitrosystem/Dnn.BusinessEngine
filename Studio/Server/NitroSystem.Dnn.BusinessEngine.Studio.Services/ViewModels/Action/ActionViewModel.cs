using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Action
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
        public bool ExecuteInClientSide { get; set; }
        public string Preconditions { get; set; }
        public string Conditions { get; set; }
        public string ActionTypeIcon { get; set; }
        public string ActionTypeTitle { get; set; }
        public string FieldType { get; set; }
        public string FieldName { get; set; }
        public string CreatedByUserDisplayName { get; set; }
        public string LastModifiedByUserDisplayName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public ActionExecutionCondition? ParentActionTriggerCondition { get; set; }
        public IEnumerable<string> AuthorizationRunAction { get; set; }
        public IEnumerable<ActionParamViewModel> Params { get; set; }
        public IEnumerable<ActionResultViewModel> Results { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}