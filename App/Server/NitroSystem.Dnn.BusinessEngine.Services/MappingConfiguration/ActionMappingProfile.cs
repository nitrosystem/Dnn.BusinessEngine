﻿using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.MappingConfiguration
{
    public static class ActionMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<ActionInfo, ActionDto>(
                (src, dest) => dest.ParentActionTriggerCondition = (ActionExecutionCondition?)src.ParentActionTriggerCondition);

            HybridMapper.BeforeMap<ActionInfo, ActionDto>(
                (src, dest) => dest.AuthorizationRunAction = src.AuthorizationRunAction?.Split(','));

            HybridMapper.BeforeMap<ActionInfo, ActionDto>(
                    (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));
        }
    }
}
