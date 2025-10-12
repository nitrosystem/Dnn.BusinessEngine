using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataServices.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution
{
    public class ActionExecutionContext : EngineContext, IEngineContext
    {
        public CancellationTokenSource CancellationTokenSource { get; }
        public override CancellationToken CancellationToken => CancellationTokenSource.Token;
        public ActionDto Action { get; }
        public ConcurrentDictionary<string, object> ModuleData { get; } // یا مدل ModuleData موجودت
        public ActionResult Result { get; set; }

        public ActionExecutionContext(ActionDto action, CancellationTokenSource ct)
        {
            Action = action;
            CancellationTokenSource = ct;
        }
    }
}
