using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.MappingConfiguration
{
    public static class ModuleMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<ModuleView, ModuleDto>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            HybridMapper.BeforeMap<ModuleView, ModuleDto>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.Wrapper);

            HybridMapper.BeforeMap<ModuleView, ModuleDto>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<ModuleFieldInfo, ModuleFieldDto>(
                (src, dest) => dest.AuthorizationViewField = src.AuthorizationViewField?.Split(','));

            HybridMapper.BeforeMap<ModuleFieldInfo, ModuleFieldDto>(
                (src, dest) => dest.ConditionalValues = ReflectionUtil.TryJsonCasting<IEnumerable<FieldValueInfo>>(src.ConditionalValues));

            HybridMapper.BeforeMap<ModuleVariableInfo, ModuleVariableDto>(
                (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);
        }
    }
}
