using System.Collections.Concurrent;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution
{
    public class ActionResponse
    {
        public ConcurrentDictionary<string, object> ModuleData { get; set; }
    }
}
