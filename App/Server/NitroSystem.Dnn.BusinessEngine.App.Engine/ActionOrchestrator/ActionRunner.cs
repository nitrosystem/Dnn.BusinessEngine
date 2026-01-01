using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionOrchestrator
{
    public class ActionRunner
    {
        private readonly IEngineRunner _engineRunner;
        private readonly IUserDataStore _userDataStore;
        private readonly IActionService _actionService;

        public ActionRunner(
           IEngineRunner engineRunner,
           IUserDataStore userDataStore,
           IActionService actionService
       )
        {
            _engineRunner = engineRunner;
            _userDataStore = userDataStore;
            _actionService = actionService;
        }

        public async Task<List<ActionResponse>> RunAsync(
            IEnumerable<Guid> actionIds,
            string connectionId,
            Guid moduleId,
            string basePath,
            ConcurrentDictionary<string, object> moduleData,
            Dictionary<string, object> extraParams = null)
        {
            var result = new List<ActionResponse>();
            var actions = await _actionService.GetActionsDtoForServerAsync(actionIds);
            var buffer = BuildActionsBuffer.BuildBuffer(actions);

            while (buffer.Any())
            {
                var node = buffer.Dequeue();
                var action = node.Action;
                var actionRequest = new ActionRequest()
                {
                    Action = action,
                    ExtraParams = extraParams,
                    ModuleData = moduleData
                };

                var engine = new ActionExecutionEngine();
                var response = await _engineRunner.RunAsync(engine, actionRequest);
                if (response.Status == ActionResultStatus.Successful && response.IsRequiredToUpdateData)
                    await _userDataStore.UpdateModuleData(connectionId, moduleId,
                        response.ModuleData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), basePath);

                result.Add(response);

                if (response.ConditionIsNotTrue) continue;

                if (response.Status == ActionResultStatus.Successful)
                {
                    EnqueueChildren(buffer, node.SuccessActions);
                }
                if (response.Status == ActionResultStatus.Error)
                {
                    EnqueueChildren(buffer, node.ErrorActions);
                }
                else
                {
                    EnqueueChildren(buffer, node.CompletedActions);
                }

                //await NotifyProgress($"Executed action {node.Action.Id}", CalculateProgress(index++, total));
            }

            return result;
        }

        private void EnqueueChildren(Queue<ActionTree> buffer, Queue<ActionTree> children)
        {
            if (children == null) return;
            while (children.TryDequeue(out var c))
                buffer.Enqueue(c);
        }
    }
}
