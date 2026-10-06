using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution
{
    public class ActionResponse
    {
        public bool ConditionIsNotTrue { get; set; }
        public bool IsRequiredToUpdateData { get; set; }
        public string RedirectUrl { get; set; }
        public ActionResultStatus Status { get; set; }
        public ConcurrentDictionary<string, object> ModuleData { get; set; }
    }
}
