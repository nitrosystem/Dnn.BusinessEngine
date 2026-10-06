using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto
{
    public class ActionDto
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? FieldId { get; set; }
        public string ActionType { get; set; }
        public string ActionName { get; set; }
        public string Event { get; set; }
        public int ExecuteOrder { get; set; }
        public string ActionConditionsDsl { get; set; }
        public string BeforeExecuteActionDsl { get; set; }
        public string ActionResultsDsl { get; set; }
        public bool IsRedirectable { get; set; }
        public ActionExecutionCondition? ParentActionTriggerCondition { get; set; }
        public IEnumerable<string> AuthorizationRunAction { get; set; }
        public IEnumerable<ActionParamDto> Params { get; set; }
        public Dictionary<string,object> Settings { get; set; }
    }
}
