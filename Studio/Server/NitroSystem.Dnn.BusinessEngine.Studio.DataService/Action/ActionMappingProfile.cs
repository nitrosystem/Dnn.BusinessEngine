using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Action;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using System.Linq;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Action
{
    public class ActionMappingProfile
    {
        public static void Register()
        {
            #region Action Type

            HybridMapper.BeforeMap<ActionTypeView, ActionTypeListItem>(
                (src, dest) => dest.Icon = src.Icon?.ReplaceFrequentTokens());

            #endregion

            #region Action 

            HybridMapper.BeforeMap<ActionInfo, ActionViewModel>(
                (src, dest) => dest.ParentActionTriggerCondition = (ActionExecutionCondition?)src.ParentActionTriggerCondition);

            HybridMapper.BeforeMap<ActionInfo, ActionViewModel>(
                (src, dest) => dest.AuthorizationRunAction = src.AuthorizationRunAction?.Split(','));

            HybridMapper.BeforeMap<ActionInfo, ActionViewModel>(
                    (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<ActionView, ActionViewModel>(
                (src, dest) => dest.ActionTypeIcon = src.ActionTypeIcon?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<ActionView, ActionViewModel>(
                (src, dest) => dest.ParentActionTriggerCondition = (ActionExecutionCondition?)src.ParentActionTriggerCondition);

            HybridMapper.BeforeMap<ActionView, ActionViewModel>(
                (src, dest) => dest.AuthorizationRunAction = src.AuthorizationRunAction?.Split(','));

            HybridMapper.BeforeMap<ActionView, ActionViewModel>(
                    (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<ActionViewModel, ActionInfo>(
                (src, dest) => dest.ParentActionTriggerCondition = (int?)src.ParentActionTriggerCondition);

            HybridMapper.BeforeMap<ActionViewModel, ActionInfo>(
                (src, dest) => dest.AuthorizationRunAction = string.Join(",", src.AuthorizationRunAction ?? Enumerable.Empty<string>()));

            HybridMapper.BeforeMap<ActionViewModel, ActionInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion
        }
    }
}
