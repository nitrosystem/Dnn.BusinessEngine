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
using NitroSystem.Dnn.BusinessEngine.App.Framework.ModuleData;

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

        public async Task<object> CallActions(IModuleData moduleData, Guid moduleId, Guid? fieldId, string eventName, bool isServerSide)
        {
            var actions = await _actionService.GetActionsDtoAsync(moduleId, fieldId, eventName, isServerSide);

            var buffer = CreateBuffer(new Queue<ActionTree>(), actions);
            if (buffer.Any())
                return await CallAction(moduleData, buffer);
            else
                return null;
        }

        //public async Task<object> CallAction(Guid actionId)
        //{
        //    var action = ActionMapping.GetActionDTO(actionId);

        //    var buffer = action.IsServerSide && action.RunChildsInServerSide ? CreateBuffer(actionId) : new Queue<ActionTree>();
        //    var node = new ActionTree()
        //    {
        //        Action = action,
        //        CompletedActions = new Queue<ActionTree>(),
        //        SuccessActions = new Queue<ActionTree>(),
        //        ErrorActions = new Queue<ActionTree>()
        //    };

        //    buffer.Enqueue(node);

        //    return await CallAction(buffer);
        //}

        public async Task<object> CallAction(IModuleData moduleData, Queue<ActionTree> buffer)
        {
            IActionResult result = null;

            var node = buffer.Dequeue();

            var action = node.Action;

            if (action != null && _actionCondition.IsTrueConditions(action.Conditions))
            {
                //ServiceDto service = null;
                //if (action.ServiceId.HasValue)
                //{
                //    service = await _serviceFactory.GetServiceDtoAsync(action.ServiceId.Value);
                //    if (!service.IsEnabled) return null;

                //    //if (service.AuthorizationRunService != null &&
                //    //    service.AuthorizationRunService.Any(role => UserController.Instance.GetCurrentUserInfo().IsInRole(role))
                //    //)
                //    //    return null;
                //}

                foreach (var item in action.Params)
                {
                    var value = _expressionService.Evaluate(moduleData, item.ParamValue) as string;
                    item.ParamValue = value;
                }

                IAction actionController = await GetActionExtensionInstance(action.ActionType);

                try
                {
                    result = await actionController.ExecuteAsync(action);

                    if (action.ServiceId.HasValue && result.Data != null) moduleData.Set("_ServiceResult", result.Data);

                    SetActionResults(moduleData, action);

                    var method = actionController.GetType().GetMethod("OnActionSuccessEvent");
                    if (method != null) method.Invoke(actionController, null);

                    if (node.SuccessActions.Any()) await CallAction(moduleData, node.SuccessActions);
                }
                catch (Exception)
                {
                    var method = actionController.GetType().GetMethod("OnActionErrorEvent");
                    if (method != null) method.Invoke(actionController, null);

                    if (node.ErrorActions.Any()) await CallAction(moduleData, node.ErrorActions);
                }
                finally
                {
                    var method = actionController.GetType().GetMethod("OnActionCompleted");
                    if (method != null) method.Invoke(actionController, null);

                    if (node.CompletedActions.Any()) await CallAction(moduleData, node.CompletedActions);
                }

                if (buffer.Any())
                    return await CallAction(moduleData, buffer);
                else
                    return result;
            }
            else
            {
                if (buffer.Any())
                    return await CallAction(moduleData, buffer);
                else
                    return result;
            }
        }

        private async Task<IAction> GetActionExtensionInstance(string actionType)
        {
            var businessControllerClass = await _actionService.GetBusinessControllerClass(actionType);

            return _serviceLocator.GetInstance<IAction>(businessControllerClass);
        }

        //private Queue<ActionTree> CreateBuffer(Guid actionId)
        //{
        //    var buffer = new Queue<ActionTree>();

        //    var action = ActionMapping.GetActionDTO(actionId);

        //    var node = new ActionTree()
        //    {
        //        Action = action,
        //        CompletedActions = new Queue<ActionTree>(),
        //        SuccessActions = new Queue<ActionTree>(),
        //        ErrorActions = new Queue<ActionTree>()
        //    };

        //    GetActionChilds(null, node.CompletedActions, action.Id, ActionResultStatus.OnCompleted);
        //    GetActionChilds(null, node.SuccessActions, action.Id, ActionResultStatus.OnCompletedSuccess);
        //    GetActionChilds(null, node.ErrorActions, action.Id, ActionResultStatus.OnCompletedError);

        //    buffer.Enqueue(node);

        //    return buffer;
        //}

        private Queue<ActionTree> CreateBuffer(Queue<ActionTree> buffer, IEnumerable<ActionDto> actions)
        {
            foreach (var action in actions ?? Enumerable.Empty<ActionDto>())
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

        private void SetActionResults(IModuleData moduleData, ActionDto action)
        {
            foreach (var item in action.Results)
            {
                var isTrue = _actionCondition.IsTrueConditions(item.Conditions);
                if (isTrue)
                {
                    var value = _expressionService.Evaluate(moduleData, item.RightExpression);
                    var setter = _expressionService.BuildJObjectSetter(item.LeftExpression);
                    setter(moduleData, value);
                }
            }
        }
    }
}
