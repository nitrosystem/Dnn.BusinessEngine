using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.App.ApplicationService.Action
{
    public static class ActionMappingProfile
    {
        public static void Register()
        {
            HybridMapper.AfterMap<ActionSpResult, ActionDto>(
                (src, dest) => dest.ParentActionTriggerCondition = (ActionExecutionCondition?)src.ParentActionTriggerCondition);

            HybridMapper.AfterMap<ActionSpResult, ActionDto>(
                (src, dest) => dest.AuthorizationRunAction = !string.IsNullOrEmpty(src.AuthorizationRunAction)
                    ? src.AuthorizationRunAction?.Split(',')
                    : null);

            HybridMapper.AfterMap<ActionSpResult, ActionDto>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));
        }
    }
}
