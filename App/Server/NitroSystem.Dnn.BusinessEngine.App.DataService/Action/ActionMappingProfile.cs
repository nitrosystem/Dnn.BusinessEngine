using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Enums;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Procedures;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.App.DataService.Action
{
    public static class ActionMappingProfile
    {
        public static void Register()
        {
            #region Action

            HybridMapper.BeforeMap<ActionSpResult, ActionDto>(
                (src, dest) => dest.ParentActionTriggerCondition = (ActionExecutionCondition?)src.ParentActionTriggerCondition);

            HybridMapper.BeforeMap<ActionSpResult, ActionDto>(
                (src, dest) => dest.AuthorizationRunAction = !string.IsNullOrEmpty(src.AuthorizationRunAction)
                    ? src.AuthorizationRunAction?.Split(',')
                    : null);

            HybridMapper.BeforeMap<ActionSpResult, ActionDto>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            #endregion

            #region Action Param

            HybridMapper.BeforeMap<ActionParamInfo, ActionParamDto>(
                (src, dest) => dest.ValueAssignmentMode = (ValueAssignmentMode)src.ValueAssignmentMode);

            #endregion
        }
    }
}
