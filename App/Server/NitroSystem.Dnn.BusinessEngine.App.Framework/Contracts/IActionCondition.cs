using System.Collections.Concurrent;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IActionCondition
    {
        bool IsTrueConditions(ConcurrentDictionary<string, object> moduleData, string conditions);
    }
}
