using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase
{
    public class EngineContext : IEngineContext
    {
        public virtual CancellationToken CancellationToken { get; }
        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

        public EngineContext(CancellationToken cancellationToken = default)
        {
            CancellationToken = cancellationToken;
        }
    }
}
