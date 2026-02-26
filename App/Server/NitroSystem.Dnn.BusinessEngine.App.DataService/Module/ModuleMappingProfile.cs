using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;

namespace NitroSystem.Dnn.BusinessEngine.App.DataService.Module
{
    public static class ModuleMappingProfile
    {
        public static void Register()
        {
            #region Module

            HybridMapper.BeforeMap<ModuleView, ModuleDto>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            HybridMapper.BeforeMap<ModuleView, ModuleDto>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.Wrapper);

            HybridMapper.BeforeMap<ModuleView, ModuleDto>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            #endregion

            #region Module Field

            HybridMapper.BeforeMap<ModuleFieldInfo, ModuleFieldDto>(
                (src, dest) => dest.AuthorizationViewField = src.AuthorizationViewField?.Split(','));

            HybridMapper.BeforeMap<ModuleFieldInfo, ModuleFieldDto>(
                (src, dest) => dest.ConditionalValues = ReflectionUtil.TryJsonCasting<IEnumerable<ModuleFieldConditionalValueDto>>(src.ConditionalValues));

            HybridMapper.BeforeMap<ModuleVariableInfo, ModuleVariableDto>(
                (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);

            HybridMapper.BeforeMap<ModuleFieldDataSourceInfo, ModuleFieldDataSourceDto>(
                (src, dest) => dest.Type = (ModuleFieldDataSourceType)src.Type);

            #endregion

            #region Module Variable

            HybridMapper.BeforeMap<ModuleVariableView, ModuleVariableDto>(
                (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);

            #endregion
        }
    }
}
