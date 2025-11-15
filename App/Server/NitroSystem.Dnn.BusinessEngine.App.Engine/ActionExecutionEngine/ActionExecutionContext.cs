using System.Threading;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionEngine
{
    public class ActionExecutionContext : EngineContext
    {
        public CancellationTokenSource CancellationTokenSource { get; }
        public override CancellationToken CancellationToken => CancellationTokenSource.Token;
        public ActionDto Action { get; set; }
        public ConcurrentDictionary<string, object> ModuleData { get; set; }
        public ActionResult Result { get; set; }

        public ActionExecutionContext(CancellationTokenSource ct)
        {
            CancellationTokenSource = ct;
        }
    }
}
