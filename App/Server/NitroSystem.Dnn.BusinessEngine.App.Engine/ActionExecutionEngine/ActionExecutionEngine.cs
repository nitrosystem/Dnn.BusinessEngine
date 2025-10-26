using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecutionEngine.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Enums;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionEngine
{
    public class ActionExecutionEngine : EngineBase<ActionRequest, ActionResponse>
    {
        private readonly EnginePipeline<ActionRequest, ActionResponse> _pipeline;

        private readonly IUserDataStore _userDataStore;
        private readonly IBuildBufferService _buildBufferService;

        private Queue<ActionTree> _buffer;
        private readonly ActionExecutionContext _ctx;

        public ActionExecutionEngine(IServiceProvider services, IUserDataStore userDataStore, IBuildBufferService buildBufferService)
            : base(services)
        {
            _userDataStore = userDataStore;
            _buildBufferService = buildBufferService;

            _pipeline = new EnginePipeline<ActionRequest, ActionResponse>()
            .Use<ActionConditionMiddleware>()
            .Use<ActionWorkerMiddleware>()
            .Use<ActionSetResultMiddleware>();

            var cts = new CancellationTokenSource();
            _ctx = new ActionExecutionContext(cts);

            OnError += OnErrorHandle;
        }

        protected override async Task OnInitializeAsync(ActionRequest request)
        {
            _ctx.ModuleData = await _userDataStore.GetOrCreateModuleDataAsync(request.ConnectionId, request.ModuleId);
            _ctx.ModuleData["_PageParam"] = UrlHelper.ParsePageParameters(request.PageUrl);

            await base.OnInitializeAsync(request);
        }

        protected override Task BeforeExecuteAsync(ActionRequest request)
        {
            _buffer = request.ByEvent
                ? _buildBufferService.BuildBufferByEvent(request.Actions.ToList())
                : _buildBufferService.BuildBuffer(request.Actions);

            return base.BeforeExecuteAsync(request);
        }

        protected override async Task<EngineResult<ActionResponse>> ExecuteCoreAsync(ActionRequest request)
        {
            try
            {
                int total = _buffer.Count;
                int index = 0;

                while (_buffer.Any())
                {
                    var node = _buffer.Dequeue();

                    _ctx.Action = node.Action;

                    await _pipeline.ExecuteAsync(request, _ctx, Services);

                    await _userDataStore.UpdateModuleData(request.ConnectionId, request.ModuleId,
                        _ctx.ModuleData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

                    if (_ctx.Result.Status == ActionResultStatus.Successful)
                    {
                        EnqueueChildren(_buffer, node.SuccessActions);
                    }
                    if (_ctx.Result.Status == ActionResultStatus.Error)
                    {
                        EnqueueChildren(_buffer, node.ErrorActions);
                    }
                    else
                    {
                        EnqueueChildren(_buffer, node.CompletedActions);
                    }

                    await NotifyProgress($"Executed action {node.Action.Id}", CalculateProgress(index++, total));
                }

                return EngineResult<ActionResponse>.Success(new ActionResponse { ModuleData = _ctx.ModuleData });

            }
            catch (Exception ex)
            {
                throw ex;
            }
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
