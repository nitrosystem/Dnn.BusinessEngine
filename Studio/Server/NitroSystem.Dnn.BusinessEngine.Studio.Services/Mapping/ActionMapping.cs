using DotNetNuke.Collections;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.TypeCasting;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;

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
                return MapActionViewModel(action, paramList, conditionList);
            });
        }

        public static ActionViewModel MapActionViewModel(ActionView action, IEnumerable<ActionParamInfo> actionParams, IEnumerable<ActionConditionInfo> conditions)
        {
            var mapper = new ExpressionMapper<ActionView, ActionViewModel>();
            mapper.AddCustomMapping(src => src, dest => dest.ActionTypeIcon, source => source.ActionTypeIcon.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions"));
            mapper.AddCustomMapping(src => src, dest => dest.Params, source => actionParams);
            mapper.AddCustomMapping(src => src, dest => dest.Conditions, source => conditions);
            mapper.AddCustomMapping(src => src, dest => dest.Results, source => GetActionResultsViewModel(source.Id));
            mapper.AddCustomMapping(src => src.Settings, dest => dest.Settings,
                    source => TypeCastingUtil<IDictionary<string, object>>.TryJsonCasting(source.Settings),
                    condition => !string.IsNullOrEmpty(condition.Settings));

            var result = mapper.Map(action);
            return result;
        }

        public static IEnumerable<ActionResultViewModel> GetActionResultsViewModel(Guid actionId)
        {
            return null;
        }

        public static ActionView MapActionInfo(ActionViewModel service)
        {
            var mapper = new ExpressionMapper<ActionViewModel, ActionView>();
            return mapper.Map(service);
        }

        #endregion
    }
}