using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts
{
    public interface IEngineContext
    {
        CancellationToken CancellationToken { get; }
        IDictionary<string, object> Items { get; }
    }
}
