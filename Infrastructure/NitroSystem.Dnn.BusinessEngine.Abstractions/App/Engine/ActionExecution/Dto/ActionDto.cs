using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Dto
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
        public bool ExecuteInClientSide { get; set; }
        public bool SetCache { get; set; }
        public bool ClearCache { get; set; }
        public string CacheKey { get; set; }
        public string Preconditions { get; set; }
        public string Conditions { get; set; }
        public int ViewOrder { get; set; }
        public ActionExecutionCondition? ParentActionTriggerCondition { get; set; }
        public IEnumerable<string> AuthorizationRunAction { get; set; }
        public IEnumerable<ActionParamDto> Params { get; set; }
        public IEnumerable<ActionResultDto> Results { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }

}
