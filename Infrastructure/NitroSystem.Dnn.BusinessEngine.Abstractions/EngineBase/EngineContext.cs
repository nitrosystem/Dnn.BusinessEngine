using System;
using System.Collections.Generic;
using System.Threading;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase
{
    // کانتکست عمومی برای هر اجرا
    public class EngineContext
    {
        public IServiceProvider Services { get; }
        public CancellationToken CancellationToken { get; }
        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

        public EngineContext(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            CancellationToken = cancellationToken;
        }

        // مثلا راحت گرفتن واحد کار
        public T GetService<T>() => (T)Services.GetService(typeof(T));
    }
}
