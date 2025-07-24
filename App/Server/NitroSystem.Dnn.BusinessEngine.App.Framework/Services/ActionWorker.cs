using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;
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
using NitroSystem.Dnn.BusinessEngine.Common.TypeCasting;
using System.Runtime.Remoting.Messaging;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto.Action;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Services
{
    public class ActionWorker : IActionWorker
    {
        private readonly IModuleData _moduleData;
        private readonly IActionService _actionService;
        private readonly IExpressionService _expressionService;
        private readonly IActionCondition _actionCondition;
        private readonly IServiceWorker _serviceWorker;
        private readonly IServiceLocator _serviceLocator;

        public ActionWorker(
            IModuleData moduleData,
            IActionService actionService,
            IExpressionService expressionService,
            IActionCondition actionCondition,
            IServiceWorker serviceWorker,
            IServiceLocator serviceLocator)
        {
            _moduleData = moduleData;
            _actionService = actionService;
            _expressionService = expressionService;
            _actionCondition = actionCondition;
            _serviceWorker = serviceWorker;
            _serviceLocator = serviceLocator;
        }

        public async Task<object> CallActions(Guid moduleId, Guid? fieldId, string eventName, bool isServerSide)
        {
            var actions = await _actionService.GetActionsDtoAsync(moduleId, fieldId, eventName, isServerSide);

            var buffer = CreateBuffer(new Queue<ActionTree>(), actions);

            if (buffer.Any())
                return await CallAction(buffer);
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

        public async Task<object> CallAction(Queue<ActionTree> buffer)
        {
            object result = null;

            var node = buffer.Dequeue();

            var action = node.Action;

            var checkConditions = action.Event == "OnPageLoad" || !action.CheckConditionsInClientSide;
            if (action != null && (!checkConditions || _actionCondition.IsTrueConditions(action.Conditions)))
            {
                ProccessActionParams(action.Params);

                IAction actionController = CreateInstance(action.ActionType);

                try
                {
                    result = await actionController.ExecuteAsync<object>();

                    var method = actionController.GetType().GetMethod("OnActionSuccessEvent");
                    if (method != null) method.Invoke(actionController, null);

                    if (node.SuccessActions.Any()) await CallAction(node.SuccessActions);
                }
                catch (Exception)
                {
                    var method = actionController.GetType().GetMethod("OnActionErrorEvent");
                    if (method != null) method.Invoke(actionController, null);

                    if (node.ErrorActions.Any()) await CallAction(node.ErrorActions);
                }
                finally
                {
                    var method = actionController.GetType().GetMethod("OnActionCompleted");
                    if (method != null) method.Invoke(actionController, null);

                    if (node.CompletedActions.Any()) await CallAction(node.CompletedActions);
                }

                if (buffer.Any())
                    return await CallAction(buffer);
                else
                    return result;
            }
            else
            {
                if (buffer.Any())
                    return await CallAction(buffer);
                else
                    return result;
            }
        }

        public IAction CreateInstance(string businessControllerClass)
        {
            //var objActionTypeInfo = ActionTypeRepository.Instance.GetActionTypeByName(actionType);

            return _serviceLocator.CreateInstance<IAction>(businessControllerClass, this, _moduleData, _expressionService, _serviceWorker);
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

        private void ProccessActionParams(IEnumerable<ActionParamInfo> actionParams)
        {
            foreach (var item in actionParams ?? Enumerable.Empty<ActionParamInfo>())
            {
                string expression = item.ParamValue != null ? item.ParamValue.ToString() : "";
                item.ParamValue = _expressionService.Evaluate(expression, _moduleData);
            }
        }

        //public void SetActionResults(ActionDto action, object data)
        //{
        //    bool isServiceBase = action.ServiceId != null;

        //    var results = ActionResultRepository.Instance.GetResults(action.Id);
        //    foreach (var item in results)
        //    {
        //        var conditions = Enumerable.Empty<ExpressionInfo>();
        //        if (!string.IsNullOrWhiteSpace(item.Conditions)) conditions = TypeCastingUtil<IEnumerable<ExpressionInfo>>.TryJsonCasting(item.Conditions);

        //        bool isTrue = _actionCondition.IsTrueConditions(conditions);

        //        if (isTrue)
        //        {
        //            object value = isServiceBase && data != null ? ProcessActionResultsToken(item.RightExpression, data, isServiceBase) : null;
        //            if (value == null)
        //                value = _expressionService.ParseExpression(item.RightExpression, _moduleData, new List<object>(), false, item.ExpressionParsingType);

        //            _moduleData.SetData(item.LeftExpression, value);
        //        }
        //    }
        //}

        //private object ProcessActionResultsToken(string expression, object data, bool isServiceBase)
        //{
        //    object result = null;

        //    if (data == null) return result;

        //    ServiceResult serviceResult = isServiceBase ? (data as ServiceResult) : null;
        //    JObject serviceData = isServiceBase ? JObject.FromObject(serviceResult) : null;

        //    var match = Regex.Match(expression, @"^(?:_ServiceResult)\.?(.[^{}:\$,]+)?$");
        //    if (match.Success && match.Groups.Count == 2)
        //    {
        //        var propertyPath = match.Groups[1].Value;

        //        if (isServiceBase && serviceData != null)
        //        {
        //            result = serviceData.SelectToken(propertyPath);
        //        }
        //    }

        //    return result;
        //}
    }
}
