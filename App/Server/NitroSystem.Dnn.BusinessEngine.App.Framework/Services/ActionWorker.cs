using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using NitroSystem.Dnn.BusinessEngine.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using NitroSystem.Dnn.BusinessEngine.Framework.Enums;
using System.Runtime.Remoting.Messaging;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using DotNetNuke.UI.UserControls;
using DotNetNuke.Abstractions.Portals;
using System.Collections.Concurrent;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Scheduling;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Services
{
    public class ActionWorker : IActionWorker
    {
        private readonly IActionService _actionService;
        private readonly IExpressionService _expressionService;
        private readonly IActionCondition _actionCondition;
        private readonly IServiceLocator _serviceLocator;

        public ActionWorker(
            IActionService actionService,
            IExpressionService expressionService,
            IActionCondition actionCondition,
            IServiceLocator serviceLocator)
        {
            _actionService = actionService;
            _expressionService = expressionService;
            _actionCondition = actionCondition;
            _serviceLocator = serviceLocator;
        }

        public async Task CallActionsAsync(ConcurrentDictionary<string, object> moduleData, Guid moduleId, Guid? fieldId, string eventName, PortalSettings portalSettings)
        {
            var actions = await _actionService.GetActionsDtoAsync(moduleId, fieldId, false);
            var actionIds = GetActionIds(actions, eventName);

            await CallActionsAsync(actionIds, moduleData, portalSettings);
        }

        public void CallActions(ConcurrentDictionary<string, object> moduleData, Guid moduleId, Guid? fieldId, string eventName, PortalSettings portalSettings)
        {
            var actions = _actionService.GetActionsDto(moduleId, fieldId, false);
            var actionIds = GetActionIds(actions, eventName);

            CallActions(actionIds, moduleData, portalSettings);
        }

        public async Task CallActionsAsync(IEnumerable<Guid> actionIds, ConcurrentDictionary<string, object> moduleData, PortalSettings portalSettings)
        {
            var actions = await _actionService.GetActionsDtoForServerAsync(actionIds);
            var buffer = BuildActionTree(actions);

            await CallActionAsync(moduleData, buffer, portalSettings);
        }

        public void CallActions(IEnumerable<Guid> actionIds, ConcurrentDictionary<string, object> moduleData, PortalSettings portalSettings)
        {
            var actions = _actionService.GetActionsDtoForServer(actionIds);
            var buffer = BuildActionTree(actions);

            CallAction(moduleData, buffer, portalSettings);
        }

        private async Task CallActionAsync(ConcurrentDictionary<string, object> moduleData, Queue<ActionTree> buffer, PortalSettings portalSettings)
        {
            if (buffer == null || buffer.Count == 0) return;

            IActionResult result = null;

            var node = buffer.Dequeue();
            var action = node.Action;

            if (action != null && _actionCondition.IsTrueConditions(moduleData, action.Conditions))
            {
                var actionParams = new List<ParamInfo>();

                foreach (var item in action.Params)
                {
                    var expr = item.ParamValue as string;
                    var value = !string.IsNullOrEmpty(expr)
                        ? _expressionService.Evaluate(expr, moduleData)
                        : item.ParamValue;

                    item.ParamValue = value;
                    actionParams.Add(item);
                }

                action.Params = actionParams;

                IAction actionController = await GetActionExtensionInstanceAsync(action.ActionType);

                try
                {
                    result = await actionController.ExecuteAsync(action, portalSettings);

                    moduleData["_ServiceResult"] = result.Data;

                    WithServiceResult(moduleData, result.Data, () =>
                    {
                        SetActionResults(moduleData, action);
                    });

                    var method = actionController.GetType().GetMethod("OnActionSuccessEvent");
                    if (method != null) method.Invoke(actionController, null);

                    await CallActionAsync(moduleData, node.SuccessActions, portalSettings);
                }
                catch (Exception)
                {
                    var method = actionController.GetType().GetMethod("OnActionErrorEvent");
                    if (method != null) method.Invoke(actionController, null);

                    await CallActionAsync(moduleData, node.ErrorActions, portalSettings);
                }
                finally
                {
                    var method = actionController.GetType().GetMethod("OnActionCompleted");
                    if (method != null) method.Invoke(actionController, null);

                    await CallActionAsync(moduleData, node.CompletedActions, portalSettings);
                }

                await CallActionAsync(moduleData, buffer, portalSettings);
            }
            else
            {
                if (buffer.Any())
                    await CallActionAsync(moduleData, buffer, portalSettings);
            }
        }

        private void CallAction(ConcurrentDictionary<string, object> moduleData, Queue<ActionTree> buffer, PortalSettings portalSettings)
        {
            if (buffer == null || buffer.Count == 0) return;

            IActionResult result = null;

            var node = buffer.Dequeue();
            var action = node.Action;

            if (action != null && _actionCondition.IsTrueConditions(moduleData, action.Conditions))
            {
                var actionParams = new List<ParamInfo>();

                foreach (var item in action.Params)
                {
                    var expr = item.ParamValue as string;
                    var value = !string.IsNullOrEmpty(expr)
                        ? _expressionService.Evaluate(expr, moduleData)
                        : item.ParamValue;

                    item.ParamValue = value;
                    actionParams.Add(item);
                }

                action.Params = actionParams;

                IAction actionController = GetActionExtensionInstance(action.ActionType);

                try
                {
                    result = actionController.Execute(action, portalSettings);

                    moduleData["_ServiceResult"] = result.Data;

                    WithServiceResult(moduleData, result.Data, () =>
                    {
                        SetActionResults(moduleData, action);
                    });

                    var method = actionController.GetType().GetMethod("OnActionSuccessEvent");
                    if (method != null) method.Invoke(actionController, null);

                    CallAction(moduleData, node.SuccessActions, portalSettings);
                }
                catch (Exception)
                {
                    var method = actionController.GetType().GetMethod("OnActionErrorEvent");
                    if (method != null) method.Invoke(actionController, null);

                    CallAction(moduleData, node.ErrorActions, portalSettings);
                }
                finally
                {
                    var method = actionController.GetType().GetMethod("OnActionCompleted");
                    if (method != null) method.Invoke(actionController, null);

                    CallAction(moduleData, node.CompletedActions, portalSettings);
                }

                CallAction(moduleData, buffer, portalSettings);
            }
            else
            {
                if (buffer.Any())
                    CallAction(moduleData, buffer, portalSettings);
            }
        }

        private List<Guid> GetActionIds(IEnumerable<ActionDto> actions, string eventName)
        {
            var actionIds = new List<Guid>();

            foreach (var action in actions.Where(a => a.Event == eventName))
            {
                var lookup = actions.Where(p => p.ParentId != null)
                    .GroupBy(a => a.ParentId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.ViewOrder).ToList());

                actionIds.AddRange(CollectActionsOptimized(action, lookup, new List<Guid>()));
            }

            return actionIds;
        }

        private IEnumerable<Guid> CollectActionsOptimized(ActionDto root, Dictionary<Guid?, List<ActionDto>> lookup, List<Guid> result)
        {
            result.Add(root.Id);

            if (lookup.TryGetValue(root.Id, out var children))
            {
                foreach (var child in children)
                {
                    CollectActionsOptimized(child, lookup, result);
                }
            }

            return result;
        }

        private IEnumerable<Guid> CollectActions(ActionDto action, IEnumerable<ActionDto> actions, List<Guid> actiondIds)
        {
            actiondIds.Add(action.Id);

            foreach (var item in actions.Where(a => a.ParentId == action.Id).OrderBy(a => a.ViewOrder))
            {
                CollectActions(item, actions, actiondIds);
            }

            return actiondIds;
        }

        private async Task<IAction> GetActionExtensionInstanceAsync(string actionType)
        {
            var businessControllerClass = await _actionService.GetBusinessControllerClassAsync(actionType);

            return _serviceLocator.GetInstance<IAction>(businessControllerClass);
        }

        private IAction GetActionExtensionInstance(string actionType)
        {
            var businessControllerClass = _actionService.GetBusinessControllerClass(actionType);

            return _serviceLocator.GetInstance<IAction>(businessControllerClass);
        }

        private Queue<ActionTree> BuildActionTree(IEnumerable<ActionDto> actions)
        {
            // ساخت دیکشنری lookup برای دسترسی سریع به ActionTreeها
            var lookup = actions.ToDictionary(a => a.Id, a => new ActionTree
            {
                Action = a,
                CompletedActions = new Queue<ActionTree>(),
                SuccessActions = new Queue<ActionTree>(),
                ErrorActions = new Queue<ActionTree>()
            });

            // نگهداری ریشه‌ها
            var roots = new Queue<ActionTree>();

            foreach (var action in actions)
            {
                if (action.ParentId == null || actions.Select(a => a.Id).Contains(action.ParentId.Value) == false)
                {
                    // ریشه‌ها
                    roots.Enqueue(lookup[action.Id]);
                }
                else
                {
                    var parentTree = lookup[action.ParentId.Value];
                    var childTree = lookup[action.Id];

                    switch (action.ParentActionTriggerCondition)
                    {
                        case ActionExecutionCondition.AlwaysExecute:
                            parentTree.CompletedActions.Enqueue(childTree);
                            break;

                        case ActionExecutionCondition.ExecuteOnSuccess:
                            parentTree.SuccessActions.Enqueue(childTree);
                            break;

                        case ActionExecutionCondition.ExecuteOnError:
                            parentTree.ErrorActions.Enqueue(childTree);
                            break;
                    }
                }
            }

            return roots;
        }

        private Queue<ActionTree> GetActionChilds(IEnumerable<ActionDto> actions, Queue<ActionTree> buffer, Guid parentId, ActionExecutionCondition parentResultStatus)
        {
            foreach (var action in actions.Where(a => a.ParentId == parentId && a.ParentActionTriggerCondition == parentResultStatus) ?? Enumerable.Empty<ActionDto>())
            {
                var node = new ActionTree()
                {
                    Action = action,
                    CompletedActions = new Queue<ActionTree>(),
                    SuccessActions = new Queue<ActionTree>(),
                    ErrorActions = new Queue<ActionTree>()
                };

                GetActionChilds(actions, node.CompletedActions, action.Id, ActionExecutionCondition.AlwaysExecute);
                GetActionChilds(actions, node.SuccessActions, action.Id, ActionExecutionCondition.ExecuteOnSuccess);
                GetActionChilds(actions, node.ErrorActions, action.Id, ActionExecutionCondition.ExecuteOnError);

                buffer.Enqueue(node);
            }

            return buffer;
        }

        private void SetActionResults(ConcurrentDictionary<string, object> moduleData, ActionDto action)
        {
            foreach (var item in action.Results)
            {
                var isTrue = _actionCondition.IsTrueConditions(moduleData, item.Conditions);
                if (isTrue)
                {
                    var value = _expressionService.Evaluate(item.RightExpression, moduleData);
                    var setter = _expressionService.BuildDataSetter(item.LeftExpression, moduleData);

                    setter(value);
                }
            }
        }

        private void WithServiceResult(
            ConcurrentDictionary<string, object> moduleData,
            object resultData,
            Action action
        )
        {
            const string key = "_ServiceResult";
            moduleData[key] = resultData;

            try
            {
                action();
            }
            finally
            {
                moduleData.TryRemove(key, out _);
            }
        }
    }
}
