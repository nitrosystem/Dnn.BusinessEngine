using System.Collections.Generic;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution
{
    public class ActionRequest
    {
        public int UserId { get; set; }
        public string BasePath { get; set; }
        public ActionDto Action { get; set; }
        public Dictionary<string, object> ExtraParams { get; set; }
        public ConcurrentDictionary<string, object> ModuleData { get; set; }
    }
}
