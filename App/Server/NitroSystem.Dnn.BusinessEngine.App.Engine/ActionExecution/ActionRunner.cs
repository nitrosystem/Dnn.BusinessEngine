using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Models;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution
{
    public class ActionRunner
    {
        private readonly IEngineRunner _engineRunner;
        private readonly IUserDataStore _userDataStore;
        private readonly ActionExecutionEngine _engine;

        public ActionRunner(
           IEngineRunner engineRunner,
           IUserDataStore userDataStore,
           ActionExecutionEngine engine)
        {
            _engineRunner = engineRunner;
            _userDataStore = userDataStore;
            _engine = engine;
        }

        public async Task<(IEnumerable<ActionResult> Results, bool IsRequiredToUpdateData)> ExecuteAsync(
            List<ActionDto> actions,
            string connectionId,
            Guid moduleId,
            string pageUrl,
            string basePath,
            int userId,
            bool isSuperUser,
            string[] roles,
            ConcurrentDictionary<string, object> moduleData,
            Dictionary<string, object> extraParams = null)
        {
            moduleData["_PageParam"] = UrlHelper.ParsePageParameters(pageUrl);
            moduleData["_CurrentUserId"] = userId;
            moduleData.TryRemove("_ActionError", out _);

            var results = new List<ActionResult>();
            var isRequiredToUpdateData = false;

            var buffer = BuildActionsBuffer.BuildBuffer(actions);
            while (buffer.Any())
            {
                var node = buffer.Dequeue();
                var action = node.Action;
                var result = new ActionResult() { Id = action.Id };

                try
                {
                    if (!isSuperUser && (action.AuthorizationRunAction != null && !action.AuthorizationRunAction.Any(r => roles.Contains(r))))
                        throw new UnauthorizedAccessException("Access Is Denied!");

                    var actionRequest = new ActionRequest()
                    {
                        UserId = userId,
                        BasePath = basePath,
                        Action = action,
                        ExtraParams = extraParams,
                        ModuleData = moduleData
                    };

                    var response = await _engineRunner.RunAsync(_engine, actionRequest);
                    if (response.Status == ActionResultStatus.Successful)
                    {
                        if (response.IsRequiredToUpdateData)
                        {
                            isRequiredToUpdateData = true;

                            moduleData = await _userDataStore.UpdateModuleDataAsync(connectionId, moduleId,
                                  response.ModuleData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), basePath);
                        }

                        if (_engine.Context.TryGet<object>("ResultData", out var data))
                            result.Data = data;
                    }
                    else if (response.ConditionIsNotTrue)
                    {
                        isRequiredToUpdateData = true;

                        moduleData = await _userDataStore.UpdateModuleDataAsync(connectionId, moduleId,
                              response.ModuleData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), basePath);

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
                catch (Exception ex)
                {
                    result.Status = ActionResultStatus.Failure;

                    isRequiredToUpdateData = true;

                    moduleData.AddOrUpdate("_ActionError", new { Code = ex.HResult, Message = ex.Message }, (key, value) => value);
                    moduleData = await _userDataStore.UpdateModuleDataAsync(connectionId, moduleId,
                            moduleData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), basePath);

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
