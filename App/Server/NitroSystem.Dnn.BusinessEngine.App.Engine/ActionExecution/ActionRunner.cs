using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution
{
    public class ActionRunner
    {
        private readonly IEngineRunner _engineRunner;
        private readonly IUserDataStore _userDataStore;
        private readonly IDiagnosticStore _diagnosticStore;

        public ActionRunner(
           IEngineRunner engineRunner,
           IUserDataStore userDataStore,
           IDiagnosticStore diagnosticStore)
        {
            _engineRunner = engineRunner;
            _userDataStore = userDataStore;
            _diagnosticStore = diagnosticStore;
        }

        public async Task<ActionResultStatus> ExecuteAsync(
            List<ActionDto> actions,
            string connectionId,
            Guid moduleId,
            int userId,
            string pageUrl,
            string basePath,
            ConcurrentDictionary<string, object> moduleData,
            Dictionary<string, object> extraParams = null,
            bool raiseException = true)
        {
            moduleData["_PageParam"] = UrlHelper.ParsePageParameters(pageUrl);
            moduleData["_CurrentUserId"] = userId;

            bool conditionIsNotTrue = false;

            try
            {
                var buffer = BuildActionsBuffer.BuildBuffer(actions);
                while (buffer.Any())
                {
                    var node = buffer.Dequeue();
                    var action = node.Action;

                    var actionRequest = new ActionRequest()
                    {
                        UserId = userId,
                        BasePath = basePath,
                        Action = action,
                        ExtraParams = extraParams,
                        ModuleData = moduleData
                    };

                    conditionIsNotTrue = false;

                    var engine = new ActionExecutionEngine(_diagnosticStore, raiseException);
                    var response = await _engineRunner.RunAsync(engine, actionRequest);
                    if (response.Status == ActionResultStatus.Successful && response.IsRequiredToUpdateData)
                        await _userDataStore.UpdateModuleData(connectionId, moduleId,
                            response.ModuleData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), basePath);

                    if (response.ConditionIsNotTrue)
                    {
                        conditionIsNotTrue = true;
                        continue;
                    }

                    if (response.Status == ActionResultStatus.Successful)
                    {
                        EnqueueChildren(buffer, node.SuccessActions);
                    }
                    if (response.Status == ActionResultStatus.Failure)
                    {
                        EnqueueChildren(buffer, node.ErrorActions);
                    }
                    else
                    {
                        EnqueueChildren(buffer, node.CompletedActions);
                    }
                }

                if (conditionIsNotTrue)
                    return ActionResultStatus.ConditionIsNotTrue;
                else
                    return ActionResultStatus.Successful;
            }
            catch (Exception ex)
            {
                if (raiseException) throw;

                return ActionResultStatus.Failure;
            }
        }

        private void EnqueueChildren(Queue<ActionTree> buffer, Queue<ActionTree> children)
        {
            if (children == null) return;
            while (children.TryDequeue(out var c))
                buffer.Enqueue(c);
        }
    }
}
