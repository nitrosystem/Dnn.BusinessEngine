using System.Collections.Concurrent;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts
{
    public interface IActionCondition
    {
        bool IsTrueConditions(ConcurrentDictionary<string, object> moduleData, string conditions);
    }
}
