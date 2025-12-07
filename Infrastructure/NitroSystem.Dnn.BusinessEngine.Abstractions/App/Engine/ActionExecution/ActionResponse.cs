using System.Collections.Concurrent;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution
{
    public class ActionResponse
    {
        public ConcurrentDictionary<string, object> ResultData { get; set; }
    }
}
