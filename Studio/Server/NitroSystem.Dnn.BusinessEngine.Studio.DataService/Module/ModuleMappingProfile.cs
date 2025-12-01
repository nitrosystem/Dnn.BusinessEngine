using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Procedures;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module
{
    public static class ModuleMappingProfile
    {
        public static void Register()
        {
            #region Module 

            HybridMapper.BeforeMap<ModuleInfo, ModuleViewModel>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            HybridMapper.BeforeMap<ModuleInfo, ModuleViewModel>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.ModuleType);

            HybridMapper.BeforeMap<ModuleInfo, ModuleViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.BeforeMap<ModuleView, ModuleViewModel>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);

            HybridMapper.BeforeMap<ModuleView, ModuleViewModel>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.Wrapper);

            HybridMapper.BeforeMap<ModuleView, ModuleViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.BeforeMap<ModuleView, ModuleDto>(
                (src, dest) => dest.Wrapper = (ModuleWrapper)src.Wrapper);

            HybridMapper.BeforeMap<ModuleViewModel, ModuleInfo>(
                (src, dest) => dest.ModuleType = (int)src.ModuleType);

            HybridMapper.BeforeMap<ModuleViewModel, ModuleInfo>(
                (src, dest) => dest.Wrapper = (int)src.Wrapper);

            HybridMapper.BeforeMap<ModuleViewModel, ModuleInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion

            #region Module Fields Types

            HybridMapper.BeforeMap<ModuleFieldTypeInfo, ModuleFieldTypeViewModel>(
                (src, dest) => dest.Icon = src.Icon?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<ModuleFieldTypeView, ModuleFieldTypeViewModel>(
                (src, dest) => dest.Icon = src.Icon?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<ModuleFieldTypeView, ModuleFieldTypeViewModel>(
                (src, dest) => dest.DefaultSettings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.DefaultSettings));

            HybridMapper.BeforeMap<ModuleFieldTypeTemplateInfo, ModuleFieldTypeTemplateViewModel>(
                (src, dest) => dest.TemplateImage = src.TemplateImage?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<ModuleFieldTypeThemeInfo, ModuleFieldTypeThemeViewModel>(
                (src, dest) => dest.ThemeImage = src.ThemeImage?.ReplaceFrequentTokens());

            #endregion

            #region Module Fields

            HybridMapper.BeforeMap<ModuleFieldInfo, ModuleFieldViewModel>(
                (src, dest) => dest.AuthorizationViewField = src.AuthorizationViewField?.Split(','));

            HybridMapper.BeforeMap<ModuleFieldInfo, ModuleFieldViewModel>(
                (src, dest) => dest.DataSource = src.HasDataSource
                ? ReflectionUtil.TryJsonCasting<ModuleFieldDataSourceInfo>(src.DataSource)
                : null
            );

            HybridMapper.BeforeMap<ModuleFieldInfo, ModuleFieldViewModel>(
                (src, dest) => dest.ConditionalValues = ReflectionUtil.TryJsonCasting<IEnumerable<ModuleFieldValueInfo>>(src.ConditionalValues));

            HybridMapper.AfterMap<ModuleFieldSpResult, ModuleFieldDto>(
                (src, dest) =>
                {
                    dest.GlobalSettings = ReflectionUtil.ConvertDictionaryToObject<ModuleFieldGlobalSettings>(dest.Settings) ?? new ModuleFieldGlobalSettings();
                });

            HybridMapper.BeforeMap<ModuleFieldViewModel, ModuleFieldInfo>(
                (src, dest) => dest.AuthorizationViewField = string.Join(",", src.AuthorizationViewField ?? Enumerable.Empty<string>()));

            HybridMapper.BeforeMap<ModuleFieldViewModel, ModuleFieldInfo>(
                (src, dest) => dest.DataSource = src.HasDataSource
                ? JsonConvert.SerializeObject(src.DataSource)
                : null)
            ;

            HybridMapper.BeforeMap<ModuleFieldViewModel, ModuleFieldInfo>(
                (src, dest) => dest.ConditionalValues = JsonConvert.SerializeObject(src.ConditionalValues));

            #endregion

            #region Module Custom Library & Resource

            HybridMapper.BeforeMap<ModuleCustomLibraryResourceView, ModuleCustomLibraryResourceViewModel>(
                (src, dest) => dest.ResourceContentType = (ResourceContentType)src.ResourceContentType);

            HybridMapper.BeforeMap<ModuleCustomLibraryResourceViewModel, ModuleCustomLibraryResourceView>(
                (src, dest) => dest.ResourceContentType = (int)src.ResourceContentType);

            #endregion

            #region Module Variable

            HybridMapper.BeforeMap<ModuleVariableInfo, ModuleVariableViewModel>(
                (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);

            HybridMapper.BeforeMap<ModuleVariableInfo, ModuleVariableListItem>(
               (src, dest) => dest.Scope = (ModuleVariableScope)src.Scope);

            HybridMapper.BeforeMap<ModuleVariableViewModel, ModuleVariableInfo>(
               (src, dest) => dest.Scope = (int)src.Scope);

            #endregion

            #region Module For Build

            HybridMapper.BeforeMap<ModuleResourceSpResult, ModuleResourceDto>(
                (src, dest) => dest.ResourcePath = src.ResourcePath?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<ModuleResourceSpResult, ModuleResourceDto>(
                (src, dest) => dest.ResourceType = (ModuleResourceType)src.ResourceType);

            HybridMapper.BeforeMap<ModuleResourceSpResult, ModuleResourceDto>(
                (src, dest) => dest.ResourceContentType = (ResourceContentType)src.ResourceContentType);

            HybridMapper.BeforeMap<ModuleResourceDto, ModuleOutputResourceInfo>(
                (src, dest) => dest.ResourceContentType = (int)src.ResourceContentType);

            #endregion
        }
    }
}
