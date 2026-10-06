using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.App.ApplicationService.Module
{
    public static class ModuleMappingProfile
    {
        public static void Register()
        {
            #region Module

            HybridMapper.AfterMap<ModuleView, ModuleDto>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            HybridMapper.AfterMap<ModuleView, ModuleDto>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.Wrapper);

            HybridMapper.AfterMap<ModuleOutputResourceInfo, ModuleOutputResourceDto>(
               (src, dest) => dest.ResourceContentType = (ResourceContentType)src.ResourceContentType);

            #endregion

            #region Module Field

            HybridMapper.AfterMap<ModuleVariableInfo, ModuleVariableDto>(
                (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);

            HybridMapper.AfterMap<ModuleFieldDataSourceInfo, ModuleFieldDataSourceDto>(
                (src, dest) => dest.Type = (ModuleFieldDataSourceType)src.Type);

            HybridMapper.AfterMap<ModuleFieldDataSourceInfo, ModuleFieldDataSourceDto>(
                (src, dest) => dest.DataStructure = (ModuleFieldDataStructure)src.DataStructure);

            #endregion

            #region Module Variable

            HybridMapper.AfterMap<ModuleVariableSpResult, ModuleVariableDto>(
                (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);

            #endregion
        }
    }
}
