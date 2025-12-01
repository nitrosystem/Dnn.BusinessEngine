using System.Threading;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase
{
    public interface IEngineContext
    {
        CancellationToken CancellationToken { get; }
        IDictionary<string, object> Items { get; }
    }
}
