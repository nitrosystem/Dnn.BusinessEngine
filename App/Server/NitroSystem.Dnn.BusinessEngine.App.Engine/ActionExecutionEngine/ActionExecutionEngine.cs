using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Middlewares;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Services;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionEngine
{
    public class ActionExecutionEngine : EngineBase<ActionRequest, ActionResponse>
    {
        private readonly EnginePipeline<ActionRequest, ActionResponse> _pipeline;

        private readonly IUserDataStore _userDataStore;

        private Queue<ActionTree> _buffer;

        public ActionExecutionEngine(IServiceProvider services)
            : base(services)
        {
            _pipeline = new EnginePipeline<ActionRequest, ActionResponse>()
            .Use<ActionConditionMiddleware>()
            .Use<ActionWorkerMiddleware>()
            .Use<ActionSetResultMiddleware>();

            OnError += OnErrorHandle;
        }

        protected override Task OnInitializeAsync(ActionRequest request)
        {
            return base.OnInitializeAsync(request);
        }

        protected override Task BeforeExecuteAsync(ActionRequest request)
        {
            _buffer = BuildBufferService.BuildBuffer(request.Actions);
            return base.BeforeExecuteAsync(request);
        }

        protected override async Task<EngineResult<ActionResponse>> ExecuteCoreAsync(ActionRequest request)
        {
            int total = _buffer.Count;
            int index = 0;

            while (_buffer.Any())
            {
                var node = _buffer.Dequeue();
                var cts = new CancellationTokenSource();
                var ctx = new ActionExecutionContext(node.Action, cts);

                await _pipeline.ExecuteAsync(request, ctx, Services);

                await _userDataStore.UpdateModuleData(request.ConnectionId, request.ModuleId,
                    ctx.ModuleData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

                if (ctx.Result.Status == ActionResultStatus.Successful)
                {
                    EnqueueChildren(_buffer, node.SuccessActions);
                }
                if (ctx.Result.Status == ActionResultStatus.Error)
                {
                    EnqueueChildren(_buffer, node.ErrorActions);
                }
                else
                {
                    EnqueueChildren(_buffer, node.CompletedActions);
                }

                await NotifyProgress($"Executed action {node.Action.Id}", CalculateProgress(index++, total));
            }

            return EngineResult<ActionResponse>.Success(new ActionResponse { });
        }

        private void EnqueueChildren(Queue<ActionTree> buffer, Queue<ActionTree> children)
        {
            if (children == null) return;
            while (children.TryDequeue(out var c))
                _buffer.Enqueue(c);
        }

        private double? CalculateProgress(int index, int total)
        {
            if (total == 0) return 100;

            double ratio = (double)index / total;
            return ratio * 100;
        }

        private Task OnErrorHandle(Exception ex, string phase)
        {
            throw new NotImplementedException();
        }
    }
}
