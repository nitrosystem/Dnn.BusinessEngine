using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Module
{
    public static class ModuleMappingProfile
    {
        public static void Register()
        {
            #region Module 

            HybridMapper.AfterMap<ModuleInfo, ModuleViewModel>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            HybridMapper.AfterMap<ModuleInfo, ModuleViewModel>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.ModuleType);

            HybridMapper.AfterMap<ModuleView, ModuleViewModel>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            HybridMapper.AfterMap<ModuleView, ModuleViewModel>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.Wrapper);

            HybridMapper.AfterMap<ModuleView, ModuleDto>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.Wrapper);

            HybridMapper.AfterMap<ModuleViewModel, ModuleInfo>(
                (src, dest) => dest.ModuleType = (int)src.ModuleType);

            HybridMapper.AfterMap<ModuleViewModel, ModuleInfo>(
                (src, dest) => dest.Wrapper = (int)src.Wrapper);

            #endregion

            #region Module Fields Types

            HybridMapper.AfterMap<ModuleFieldTypeInfo, ModuleFieldTypeViewModel>(
                (src, dest) => dest.Icon = src.Icon?.ReplaceFrequentTokens());

            HybridMapper.AfterMap<ModuleFieldTypeInfo, ModuleFieldTypeViewModel>(
                (src, dest) => dest.DefaultSettings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.DefaultSettings, true));

            HybridMapper.AfterMap<ModuleFieldTypeTemplateInfo, ModuleFieldTypeTemplateViewModel>(
                (src, dest) => dest.TemplateImage = src.TemplateImage?.ReplaceFrequentTokens());

            HybridMapper.AfterMap<ModuleFieldTypeThemeInfo, ModuleFieldTypeThemeViewModel>(
                (src, dest) => dest.ThemeImage = src.ThemeImage?.ReplaceFrequentTokens());

            #endregion

            #region Module Field Data Source

            HybridMapper.AfterMap<ModuleFieldDataSourceInfo, ModuleFieldDataSourceViewModel>(
               (src, dest) => dest.Type = (ModuleFieldDataSourceType)src.Type);

            HybridMapper.AfterMap<ModuleFieldDataSourceInfo, ModuleFieldDataSourceViewModel>(
               (src, dest) => dest.DataStructure = (ModuleFieldDataStructure)src.DataStructure);

            HybridMapper.AfterMap<ModuleFieldDataSourceViewModel, ModuleFieldDataSourceInfo>(
               (src, dest) => dest.Type = (int)src.Type);

            #endregion

            #region Module Custom Library & Resource

            HybridMapper.AfterMap<ModuleCustomLibraryView, ModuleCustomLibraryViewModel>(
                (src, dest) => dest.Logo = src.Logo?.ReplaceFrequentTokens());

            HybridMapper.AfterMap<ModuleCustomLibraryResourceView, ModuleCustomLibraryResourceViewModel>(
                (src, dest) => dest.ResourceContentType = (ResourceContentType)src.ResourceContentType);

            HybridMapper.AfterMap<ModuleCustomLibraryResourceViewModel, ModuleCustomLibraryResourceView>(
                (src, dest) => dest.ResourceContentType = (int)src.ResourceContentType);

            HybridMapper.AfterMap<ModuleCustomResourceInfo, ModuleCustomResourceViewModel>(
                (src, dest) => dest.ResourceContentType = (ResourceContentType)src.ResourceContentType);

            HybridMapper.AfterMap<ModuleCustomResourceViewModel, ModuleCustomResourceInfo>(
                (src, dest) => dest.ResourceContentType = (int)src.ResourceContentType);

            #endregion

            #region Module Variable

            HybridMapper.AfterMap<ModuleVariableInfo, ModuleVariableViewModel>(
              (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);

            HybridMapper.AfterMap<ModuleVariableInfo, ModuleVariableListItem>(
               (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);

            HybridMapper.AfterMap<ModuleVariableViewModel, ModuleVariableInfo>(
               (src, dest) => dest.Scope = (int)src.Scope);

            #endregion

            #region Module For Build

            HybridMapper.AfterMap<ModuleFieldDataSourceSpResult, ModuleFieldDataSourceDto>(
                (src, dest) => dest.Type = (ModuleFieldDataSourceType)src.Type);

            HybridMapper.AfterMap<ModuleResourceSpResult, ModuleResourceDto>(
                (src, dest) => dest.ResourcePath = src.ResourcePath?.ReplaceFrequentTokens());

            HybridMapper.AfterMap<ModuleResourceSpResult, ModuleResourceDto>(
                (src, dest) => dest.ResourceType = (ModuleResourceType)src.ResourceType);

            HybridMapper.AfterMap<ModuleResourceSpResult, ModuleResourceDto>(
                (src, dest) => dest.ResourceContentType = (ResourceContentType)src.ResourceContentType);

            HybridMapper.AfterMap<ModuleResourceDto, ModuleOutputResourceInfo>(
                (src, dest) => dest.ResourceContentType = (int)src.ResourceContentType);

            #endregion
        }
    }
}
