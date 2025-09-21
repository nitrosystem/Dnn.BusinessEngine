using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Shared.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Dto
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
        public ActionExecutionCondition? ParentActionTriggerCondition { get; set; }
        public bool ExecuteInClientSide { get; set; }
        public string Preconditions { get; set; }
        public string Conditions { get; set; }
        public IDictionary<string, object> Settings { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<string> AuthorizationRunAction { get; set; }
        public IEnumerable<ParamInfo> Params { get; set; }
        public IEnumerable<ActionResultDto> Results { get; set; }
    }

}
