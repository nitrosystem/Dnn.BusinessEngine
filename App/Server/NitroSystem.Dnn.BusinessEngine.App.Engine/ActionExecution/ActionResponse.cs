using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Models;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution
{
    public class ActionResponse
    {
        public bool ConditionIsNotTrue { get; set; }
        public bool IsRequiredToUpdateData { get; set; }
        public ActionResultStatus Status { get; set; }
        public object ActionResultData { get; set; }
        public ConcurrentDictionary<string, object> ModuleData { get; set; }
        public string CompletionPhase { get; set; }
        public string CompletionMiddleware { get; set; }
        public ActionException ErrorException { get; set; }
    }
}
