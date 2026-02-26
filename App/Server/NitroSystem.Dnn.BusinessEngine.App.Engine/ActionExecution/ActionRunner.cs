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
using NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate;

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

        public async Task<(IEnumerable<ActionResult> Results, bool IsRequiredToUpdateData)> ExecuteAsync(
            List<ActionDto> actions,
            string connectionId,
            Guid moduleId,
            int userId,
            string pageUrl,
            string basePath,
            ConcurrentDictionary<string, object> moduleData,
            Dictionary<string, object> extraParams = null)
        {
            moduleData["_PageParam"] = UrlHelper.ParsePageParameters(pageUrl);
            moduleData["_CurrentUserId"] = userId;

            var results = new List<ActionResult>();
            var isRequiredToUpdateData = false;

            var buffer = BuildActionsBuffer.BuildBuffer(actions);
            while (buffer.Any())
            {
                var result = new ActionResult();
                var node = buffer.Dequeue();

                try
                {
                    var action = node.Action;
                    var actionRequest = new ActionRequest()
                    {
                        UserId = userId,
                        BasePath = basePath,
                        Action = action,
                        ExtraParams = extraParams,
                        ModuleData = moduleData
                    };

                    var engine = new ActionExecutionEngine(_diagnosticStore);
                    var response = await _engineRunner.RunAsync(engine, actionRequest);

                    if (response.Status == ActionResultStatus.Successful && response.IsRequiredToUpdateData)
                    {
                        isRequiredToUpdateData = true;

                        moduleData = await _userDataStore.UpdateModuleData(connectionId, moduleId,
                              response.ModuleData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), basePath);
                    }
                    else if (response.ConditionIsNotTrue)
                    {
                        result.Status = ActionResultStatus.ConditionIsNotTrue;
                        continue;
                    }

                    if (action.IsRedirectable && !string.IsNullOrEmpty(response.RedirectUrl))
                    {
                        result.IsRedirectable = true;
                        result.RedirectUrl = response.RedirectUrl;
                    }

                    result.Status = ActionResultStatus.Successful;

                    EnqueueChildren(buffer, node.SuccessActions);
                }
                catch (Exception)
                {
                    result.Status = ActionResultStatus.Failure;

                    EnqueueChildren(buffer, node.ErrorActions);
                }
                finally
                {
                    results.Add(result);

                    EnqueueChildren(buffer, node.CompletedActions);
                }
            }

            return (results, isRequiredToUpdateData);
        }

        private void EnqueueChildren(Queue<ActionTree> buffer, Queue<ActionTree> children)
        {
            if (children == null) return;
            while (children.TryDequeue(out var c))
                buffer.Enqueue(c);
        }
    }
}
