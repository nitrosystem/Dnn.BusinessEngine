using DotNetNuke.Collections;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using DotNetNuke.Common.Utilities;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationActions.Mapping
{
    public static class ActionMapping
    {
        //#region Action Type Mapping

        //public static IEnumerable<ActionTypeViewModel> GetActionTypesViewModel()
        //{
        //    var serviceTypes = ActionTypeRepository.Instance.GetActionTypes();

        //    return GetActionTypesViewModel(serviceTypes);
        //}

        //public static IEnumerable<ActionTypeViewModel> GetActionTypesViewModel(IEnumerable<ActionTypeView> serviceTypes)
        //{
        //    var result = new List<ActionTypeViewModel>();

        //    foreach (var objActionTypeView in serviceTypes ?? Enumerable.Empty<ActionTypeView>())
        //    {
        //        var serviceType = GetActionTypeViewModel(objActionTypeView);
        //        result.Add(serviceType);
        //    }

        //    return result;
        //}

        //public static ActionTypeViewModel GetActionTypeViewModel(ActionTypeView objActionTypeView)
        //{

        //}

        //#endregion

        #region Action Mapping

        public static IEnumerable<ActionViewModel> MapActionsViewModel(IEnumerable<ActionView> actions, IEnumerable<ActionParamInfo> actionParams, IEnumerable<ActionConditionInfo> conditions)
        {
            var paramsDict = actionParams.GroupBy(c => c.ActionId)
                         .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var conditionsDict = conditions.GroupBy(c => c.ActionId)
                         .ToDictionary(g => g.Key, g => g.AsEnumerable());

            return actions.Select(action =>
            {
                var paramList = paramsDict.TryGetValue(action.Id, out var cols1) ? cols1 : Enumerable.Empty<ActionParamInfo>();
                var conditionList = conditionsDict.TryGetValue(action.Id, out var cols2) ? cols2 : Enumerable.Empty<ActionConditionInfo>();
                return MapActionViewModel(action, null, conditionList, paramList);
            });
        }

        public static ActionViewModel MapActionViewModel(ActionView action, IEnumerable<ActionResultInfo> actionResults, IEnumerable<ActionConditionInfo> conditions, IEnumerable<ActionParamInfo> actionParams)
        {
            var mapper = new ExpressionMapper<ActionView, ActionViewModel>();
            mapper.AddCustomMapping(src => src, dest => dest.ActionTypeIcon, src => src.ActionTypeIcon.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions"));
            mapper.AddCustomMapping(src => src, dest => dest.Params, src => actionParams);
            mapper.AddCustomMapping(src => src, dest => dest.Conditions, src => conditions);
            mapper.AddCustomMapping(src => src, dest => dest.Results, src => GetActionResultsViewModel(actionResults));
            mapper.AddCustomMapping(src => src.Settings, dest => dest.Settings,
                src => TypeCasting.TryJsonCasting<IDictionary<string, object>>(src.Settings),
                condition => !string.IsNullOrEmpty(condition.Settings));

            var result = mapper.Map(action);
            return result;
        }

        public static IEnumerable<ActionResultViewModel> GetActionResultsViewModel(IEnumerable<ActionResultInfo> actionResults)
        {
            return (actionResults ?? Enumerable.Empty<ActionResultInfo>()).Select(result =>
            {
                var mapper = new ExpressionMapper<ActionResultInfo, ActionResultViewModel>();
                mapper.AddCustomMapping(src => src.Conditions, dest => dest.Conditions,
                        src => TypeCasting.TryJsonCasting<IEnumerable<ExpressionInfo>>(src.Conditions),
                        condition => !string.IsNullOrEmpty(condition.Conditions));
                return mapper.Map(result);
            });
        }

        public static (
            ActionInfo Action,
            IEnumerable<ActionResultInfo> Results,
            IEnumerable<ActionConditionInfo> Conditions,
            IEnumerable<ActionParamInfo> Params)
            MapActionInfoWithChilds(ActionViewModel action)
        {
            var actionMapper = new ExpressionMapper<ActionViewModel, ActionInfo>();
            actionMapper.AddCustomMapping(src => src.Settings, dest => dest.Settings, src => JsonConvert.SerializeObject(src.Settings));
            var objActionInfo = actionMapper.Map(action);

            var actionResults = (action.Results ?? Enumerable.Empty<ActionResultViewModel>()).Select(result =>
                {
                    var actionResultMapper = new ExpressionMapper<ActionResultViewModel, ActionResultInfo>();
                    actionResultMapper.AddCustomMapping(src => src.Conditions, dest => dest.Conditions, src => JsonConvert.SerializeObject(src.Conditions));
                    return actionResultMapper.Map(result);
                });

            var actionParams = action.Params ?? Enumerable.Empty<ActionParamInfo>();
            var actionConditions = action.Conditions ?? Enumerable.Empty<ActionConditionInfo>();

            return (objActionInfo, actionResults, actionConditions, actionParams);
        }

        #endregion
    }
}