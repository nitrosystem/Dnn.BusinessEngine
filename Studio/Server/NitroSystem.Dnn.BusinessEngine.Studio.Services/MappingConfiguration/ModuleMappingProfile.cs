using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.MappingConfiguration
{
    public static class ModuleMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<ModuleInfo, ModuleViewModel>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            HybridMapper.BeforeMap<ModuleView, ModuleViewModel>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            HybridMapper.BeforeMap<ModuleView, ModuleViewModel>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.Wrapper);

            HybridMapper.BeforeMap<ModuleView, ModuleViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.AfterMap<ModuleView, BuildModuleDto>(
                (src, dest) => dest.BuildPath = $@"[BUILDPATH]\{src.ScenarioName}\{src.ModuleName}");

            HybridMapper.AfterMap<ModuleFieldInfo, ModuleFieldViewModel>(
                (src, dest) => dest.AuthorizationViewField = src.AuthorizationViewField?.Split(','));

            HybridMapper.AfterMap<ModuleFieldInfo, ModuleFieldViewModel>(
                (src, dest) => dest.DataSource = ReflectionUtil.TryJsonCasting<FieldDataSourceInfo>(src.DataSource));

            HybridMapper.AfterMap<ModuleFieldInfo, ModuleFieldViewModel>(
                (src, dest) => dest.ConditionalValues = ReflectionUtil.TryJsonCasting<IEnumerable<FieldValueInfo>>(src.ConditionalValues));

            HybridMapper.BeforeMap<ModuleVariableInfo, ModuleVariableViewModel>(
                (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);

            HybridMapper.BeforeMap<ModuleVariableInfo, ModuleVariableDto>(
               (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);

            HybridMapper.BeforeMap<ModuleVariableViewModel, ModuleVariableInfo>(
               (src, dest) => dest.Scope = (int)src.Scope);

            HybridMapper.BeforeMap<BuildModuleResourceDto, MachineResourceFileInfo>(
               (src, dest) => dest.ContinueOnError =true);
        }
    }
}
