using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Action;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.MappingConfiguration
{
    public class ActionMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<ActionTypeView, ActionTypeListItem>(
                (src, dest) => dest.Icon = src.Icon?.ReplaceFrequentTokens());

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
                (src, dest) => dest.AuthorizationRunAction = string.Join(",", src.AuthorizationRunAction));

            HybridMapper.BeforeMap<ActionViewModel, ActionInfo>(
                    (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));
        }
    }
}
