using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.DataService.Action
{
    public static class ActionMappingProfile
    {
        public static void Register()
        {
            #region Action

            HybridMapper.BeforeMap<ActionInfo, ActionDto>(
                (src, dest) => dest.ParentActionTriggerCondition = (ActionExecutionCondition?)src.ParentActionTriggerCondition);

            HybridMapper.BeforeMap<ActionInfo, ActionDto>(
                (src, dest) => dest.AuthorizationRunAction = src.AuthorizationRunAction?.Split(','));

            #endregion

            #region Action Param

            HybridMapper.BeforeMap<ActionParamInfo, ActionParamDto>(
                (src, dest) => dest.ValueAssignmentMode = (ValueAssignmentMode)src.ValueAssignmentMode);

            #endregion
        }
    }
}
