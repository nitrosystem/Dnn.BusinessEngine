using System.Threading;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts
{
    public interface IEngineContext
    {
        EngineExecutionPhase CurrentPhase { get; set; }
        string CurrentMiddleware { get; set; }
        CancellationTokenSource CancellationTokenSource { get; }

        void Set<T>(string key, T value);
        T Get<T>(string key);
        bool TryGet<T>(string key, out T value);
    }
}
