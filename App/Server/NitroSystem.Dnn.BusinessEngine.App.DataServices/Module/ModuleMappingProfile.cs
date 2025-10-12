using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataServices.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataServices.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.DataServices.Module
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
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            #endregion

            #region Module Field

            HybridMapper.BeforeMap<ModuleFieldInfo, ModuleFieldDto>(
                (src, dest) => dest.AuthorizationViewField = src.AuthorizationViewField?.Split(','));

            HybridMapper.BeforeMap<ModuleFieldInfo, ModuleFieldDto>(
                (src, dest) => dest.ConditionalValues = ReflectionUtil.TryJsonCasting<IEnumerable<FieldValueInfo>>(src.ConditionalValues));

            HybridMapper.BeforeMap<ModuleVariableInfo, ModuleVariableDto>(
                (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);

            #endregion
        }
    }
}
