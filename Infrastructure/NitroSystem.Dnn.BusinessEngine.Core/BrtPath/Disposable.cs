using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.BrtPath
{
    // helper disposable (بدون dependency)
    public static class Disposable
    {
        public static IDisposable Create(Action dispose)
            => new ActionDisposable(dispose);

        private sealed class ActionDisposable : IDisposable
        {
            private Action? _dispose;
            public ActionDisposable(Action d) => _dispose = d;
            public void Dispose() { Interlocked.Exchange(ref _dispose, null)?.Invoke(); }
        }
    }
}
