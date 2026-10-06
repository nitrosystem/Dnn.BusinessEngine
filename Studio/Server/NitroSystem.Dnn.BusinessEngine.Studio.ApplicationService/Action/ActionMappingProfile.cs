using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Action;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Action
{
    public class ActionMappingProfile
    {
        public static void Register()
        {
            #region Action Type

            HybridMapper.AfterMap<ActionTypeInfo, ActionTypeListItem>(
                (src, dest) => dest.Icon = src.Icon?.ReplaceFrequentTokens());

            #endregion

            #region Action 

            HybridMapper.AfterMap<ActionInfo, ActionViewModel>(
                (src, dest) => dest.ParentActionTriggerCondition = (ActionExecutionCondition?)src.ParentActionTriggerCondition);

            HybridMapper.AfterMap<ActionInfo, ActionViewModel>(
                (src, dest) => dest.AuthorizationRunAction = src.AuthorizationRunAction?.Split(','));

            HybridMapper.AfterMap<ActionInfo, ActionViewModel>(
                    (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.AfterMap<ActionView, ActionViewModel>(
                (src, dest) => dest.ActionTypeIcon = src.ActionTypeIcon?.ReplaceFrequentTokens());

            HybridMapper.AfterMap<ActionView, ActionViewModel>(
                (src, dest) => dest.ParentActionTriggerCondition = (ActionExecutionCondition?)src.ParentActionTriggerCondition);

            HybridMapper.AfterMap<ActionView, ActionViewModel>(
                (src, dest) => dest.AuthorizationRunAction = src.AuthorizationRunAction?.Split(','));

            HybridMapper.AfterMap<ActionView, ActionViewModel>(
                    (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.AfterMap<ActionViewModel, ActionInfo>(
                (src, dest) => dest.ParentActionTriggerCondition = (int?)src.ParentActionTriggerCondition);

            HybridMapper.AfterMap<ActionViewModel, ActionInfo>(
                (src, dest) => dest.AuthorizationRunAction = string.Join(",", src.AuthorizationRunAction ?? Enumerable.Empty<string>()));

            HybridMapper.AfterMap<ActionViewModel, ActionInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion
        }
    }
}
