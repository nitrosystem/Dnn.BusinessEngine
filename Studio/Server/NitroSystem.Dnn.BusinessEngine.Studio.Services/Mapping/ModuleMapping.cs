using DotNetNuke.Entities.Portals;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using DotNetNuke.UI.WebControls;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module.Field;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models;
using NitroSystem.Dnn.BusinessEngine.Core.General;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping
{
    public static class ModuleMapping
    {
        #region Module Mapping

        public static IEnumerable<ModuleViewModel> MapModulesViewModel(IEnumerable<ModuleView> modules, IEnumerable<(Guid moduleId, string customHtml, string customJs, string customCss)> customFiles)
        {
            var mergedList = customFiles
                .Join(modules, file => file.moduleId, module => module.Id, (file, module) => (module, file));

            return mergedList.Select(tuple => MapModuleViewModel(tuple.module));
        }

        public static ModuleViewModel MapModuleViewModel(ModuleView module)
        {
            var mapper = new ExpressionMapper<ModuleView, ModuleViewModel>();
            mapper.AddCustomMapping(src => src.Settings, dest => dest.Settings, src =>
                    TypeCasting.TryJsonCasting<IDictionary<string, object>>(src.Settings.ToString()),
                    condition => !string.IsNullOrWhiteSpace(condition.Settings));

            var result = mapper.Map(module);
            return result;
        }

        public static ModuleInfo MapModuleInfo(ModuleViewModel module)
        {
            var mapper = new ExpressionMapper<ModuleViewModel, ModuleInfo>();
            mapper.AddCustomMapping(src => src.Settings, dest => dest.Settings, src => JsonConvert.SerializeObject(src.Settings));

            var result = mapper.Map(module);
            return result;
        }

        public static ModuleDto MapModuleDto(ModuleView module)
        {
            var mapper = new ExpressionMapper<ModuleView, ModuleDto>();

            var result = mapper.Map(module);
            return result;
        }

        #endregion

        #region Module Field Type Mapping

        public static IEnumerable<ModuleFieldTypeViewModel> MapModuleFieldTypesViewModel(IEnumerable<ModuleFieldTypeView> fieldTypes, IEnumerable<ModuleFieldTypeTemplateInfo> templates, IEnumerable<ModuleFieldTypeThemeInfo> themes)
        {
            var templateDict = templates.GroupBy(t => t.FieldType).
                        ToDictionary(t => t.Key, t => t.AsEnumerable());

            var themeDict = themes.GroupBy(t => t.FieldType).
                        ToDictionary(t => t.Key, t => t.AsEnumerable());

            return fieldTypes.Select(fieldtype =>
              {
                  var templateItems = templateDict.TryGetValue(fieldtype.FieldType, out var items1) ? items1 : Enumerable.Empty<ModuleFieldTypeTemplateInfo>();
                  var themeItems = themeDict.TryGetValue(fieldtype.FieldType, out var items2) ? items2 : Enumerable.Empty<ModuleFieldTypeThemeInfo>();

                  return MapModuleFieldTypeViewModel(fieldtype, templateItems, themeItems);
              });
        }

        public static ModuleFieldTypeViewModel MapModuleFieldTypeViewModel(ModuleFieldTypeView fieldtype, IEnumerable<ModuleFieldTypeTemplateInfo> templates, IEnumerable<ModuleFieldTypeThemeInfo> themes)
        {
            var mapper = new ExpressionMapper<ModuleFieldTypeView, ModuleFieldTypeViewModel>();
            mapper.AddCustomMapping(src => src.Icon, dest => dest.Icon, src => src.Icon.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions"));
            mapper.AddCustomMapping(src => src.DefaultSettings, dest => dest.DefaultSettings, src => TypeCasting.TryJsonCasting<IDictionary<string, object>>(src.DefaultSettings.ToString()));
            mapper.AddCustomMapping(src => src, dest => dest.Templates,
                src => MapModuleFieldTypeTemplatesViewModel(templates),
                condition => templates != null && templates.Any()
            );
            mapper.AddCustomMapping(src => src, dest => dest.Themes,
                src => MapModuleFieldTypeThemesViewModel(themes),
                condition => themes != null && themes.Any()
            );

            var result = mapper.Map(fieldtype);
            return result;
        }

        public static IEnumerable<ModuleFieldTypeTemplateViewModel> MapModuleFieldTypeTemplatesViewModel(IEnumerable<ModuleFieldTypeTemplateInfo> templates)
        {
            return templates.Select(
                template => MapModuleFieldTypeTemplateViewModel(template)
            );
        }

        public static ModuleFieldTypeTemplateViewModel MapModuleFieldTypeTemplateViewModel(ModuleFieldTypeTemplateInfo template)
        {
            var mapper = new ExpressionMapper<ModuleFieldTypeTemplateInfo, ModuleFieldTypeTemplateViewModel>();
            mapper.AddCustomMapping(src => src.TemplatePath, dest => dest.TemplatePath,
                src => src.TemplatePath.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions"),
                condition => !string.IsNullOrWhiteSpace(condition.TemplatePath)
            );
            mapper.AddCustomMapping(src => src.TemplatePath, dest => dest.TemplatePath,
                src => src.TemplateImage.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions"),
                condition => !string.IsNullOrWhiteSpace(condition.TemplatePath)
            );

            var result = mapper.Map(template);
            return result;
        }

        public static IEnumerable<ModuleFieldTypeThemeViewModel> MapModuleFieldTypeThemesViewModel(IEnumerable<ModuleFieldTypeThemeInfo> themes)
        {
            return themes.Select(
                theme => MapModuleFieldTypeThemeViewModel(theme)
            );
        }

        public static ModuleFieldTypeThemeViewModel MapModuleFieldTypeThemeViewModel(ModuleFieldTypeThemeInfo theme)
        {
            var mapper = new ExpressionMapper<ModuleFieldTypeThemeInfo, ModuleFieldTypeThemeViewModel>();
            mapper.AddCustomMapping(src => src.ThemeImage, dest => dest.ThemeImage,
                src => src.ThemeImage.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions"),
                condition => !string.IsNullOrWhiteSpace(condition.ThemeImage)
            );
            mapper.AddCustomMapping(src => src.ThemeCssPath, dest => dest.ThemeCssPath,
                src => src.ThemeCssPath.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions"),
                condition => !string.IsNullOrWhiteSpace(condition.ThemeCssPath)
            );

            var result = mapper.Map(theme);
            return result;
        }

        #endregion

        #region Module Field Mapping

        public static IEnumerable<ModuleFieldViewModel> MapModuleFieldsViewModel(IEnumerable<ModuleFieldInfo> fields, IEnumerable<ModuleFieldSettingView> settings)
        {
            var settingsDict = settings.GroupBy(c => c.FieldId)
                                     .ToDictionary(g => g.Key, g => g.AsEnumerable());

            return fields.Select(field =>
            {
                var items = settingsDict.TryGetValue(field.Id, out var fieldSettings) ? fieldSettings : Enumerable.Empty<ModuleFieldSettingView>();
                return MapModuleFieldViewModel(field, items);
            });
        }

        public static ModuleFieldViewModel MapModuleFieldViewModel(ModuleFieldInfo field, IEnumerable<ModuleFieldSettingView> settings)
        {
            var mapper = new ExpressionMapper<ModuleFieldInfo, ModuleFieldViewModel>();
            mapper.AddCustomMapping(src => src.AuthorizationViewField, dest => dest.AuthorizationViewField,
                src => src.AuthorizationViewField.Split(','),
                condition => !string.IsNullOrEmpty(condition.AuthorizationViewField));
            mapper.AddCustomMapping(src => src.ShowConditions, dest => dest.ShowConditions, src => TypeCasting.TryJsonCasting<IEnumerable<ExpressionInfo>>(src.ShowConditions));
            mapper.AddCustomMapping(src => src.EnableConditions, dest => dest.EnableConditions, src => TypeCasting.TryJsonCasting<IEnumerable<ExpressionInfo>>(src.EnableConditions));
            mapper.AddCustomMapping(src => src.FieldValues, dest => dest.FieldValues, src => TypeCasting.TryJsonCasting<IEnumerable<FieldValueInfo>>(src.FieldValues));
            mapper.AddCustomMapping(src => src.DataSource, dest => dest.DataSource, src => TypeCasting.TryJsonCasting<FieldDataSourceInfo>(src.DataSource));
            mapper.AddCustomMapping(src => src, dest => dest.Settings, src => MapModuleFieldSettingsToDictionary(settings));

            var result = mapper.Map(field);
            return result;
        }

        public static IDictionary<string, object> MapModuleFieldSettingsToDictionary(IEnumerable<ModuleFieldSettingView> settings)
        {
            return settings.ToDictionary(
                x => x.SettingName,
                x => Globals.ConvertStringToObject(x.SettingValue)
            );
        }

        public static ModuleFieldInfo MapModuleFieldInfo(ModuleFieldViewModel field)
        {
            var mapper = new ExpressionMapper<ModuleFieldViewModel, ModuleFieldInfo>();
            mapper.AddCustomMapping(src => src.AuthorizationViewField, dest => dest.AuthorizationViewField,
                src => string.Join(",", src.AuthorizationViewField),
                condition => condition.AuthorizationViewField != null && condition.AuthorizationViewField.Any()
            );
            mapper.AddCustomMapping(src => src.ShowConditions, dest => dest.ShowConditions,
                src => JsonConvert.SerializeObject(src.ShowConditions),
                condition => condition.ShowConditions != null && condition.ShowConditions.Any()
            );
            mapper.AddCustomMapping(src => src.EnableConditions, dest => dest.EnableConditions,
                src => JsonConvert.SerializeObject(src.EnableConditions),
                condition => condition.EnableConditions != null && condition.EnableConditions.Any()
            );
            mapper.AddCustomMapping(src => src.FieldValues, dest => dest.FieldValues,
               src => JsonConvert.SerializeObject(src.FieldValues),
               condition => condition.FieldValues != null && condition.FieldValues.Any()
           );
            mapper.AddCustomMapping(src => src.DataSource, dest => dest.DataSource,
               src => JsonConvert.SerializeObject(src.DataSource),
               condition => condition.DataSource != null
           );

            var result = mapper.Map(field);
            return result;
        }

        public static IEnumerable<BuildModuleFieldDto> MapModuleFieldsDto(IEnumerable<ModuleFieldInfo> fields, IEnumerable<ModuleFieldSettingView> settings)
        {
            var settingsDict = settings.GroupBy(c => c.FieldId)
                                     .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var result = new List<BuildModuleFieldDto>();

            foreach (var field in fields)
            {
                var items = settingsDict.TryGetValue(field.Id, out var fieldSettings) ? fieldSettings : Enumerable.Empty<ModuleFieldSettingView>();

                result.Add(MapModuleFieldDto(field, items));
            }

            return result;
        }

        public static BuildModuleFieldDto MapModuleFieldDto(ModuleFieldInfo field, IEnumerable<ModuleFieldSettingView> settings)
        {
            var dictionarySettings = MapModuleFieldSettingsToDictionary(settings);

            var mapper = new ExpressionMapper<ModuleFieldInfo, BuildModuleFieldDto>();
            mapper.AddCustomMapping(src => src.AuthorizationViewField, dest => dest.AuthorizationViewField,
                src => src.AuthorizationViewField.Split(','),
                condition => !string.IsNullOrEmpty(condition.AuthorizationViewField));
            mapper.AddCustomMapping(src => src.ShowConditions, dest => dest.ShowConditions, src => TypeCasting.TryJsonCasting<IEnumerable<ExpressionInfo>>(src.ShowConditions));
            mapper.AddCustomMapping(src => src.EnableConditions, dest => dest.EnableConditions, src => TypeCasting.TryJsonCasting<IEnumerable<ExpressionInfo>>(src.EnableConditions));
            mapper.AddCustomMapping(src => src.FieldValues, dest => dest.FieldValues, src => TypeCasting.TryJsonCasting<IEnumerable<FieldValueInfo>>(src.FieldValues));
            mapper.AddCustomMapping(src => src.DataSource, dest => dest.DataSource, src => TypeCasting.TryJsonCasting<FieldDataSourceInfo>(src.DataSource));
            mapper.AddCustomMapping(src => src, dest => dest.Settings, src => dictionarySettings);
            mapper.AddCustomMapping(src => src, dest => dest.GlobalSettings,
                src => DictionaryToObjectConverter.ConvertToObject<ModuleFieldGlobalSettings>(dictionarySettings) ?? new ModuleFieldGlobalSettings());

            var result = mapper.Map(field);
            return result;
        }

        #endregion

        #region Module Variable Mapping

        public static IEnumerable<ModuleVariableViewModel> MapModuleVariablesViewModel(IEnumerable<ModuleVariableInfo> variables, IEnumerable<ViewModelInfo> viewModels)
        {
            return variables.Select(variable => MapModuleVariableViewModel(variable, viewModels));
        }

        public static ModuleVariableViewModel MapModuleVariableViewModel(ModuleVariableInfo variable, IEnumerable<ViewModelInfo> viewModels)
        {
            var mapper = new ExpressionMapper<ModuleVariableInfo, ModuleVariableViewModel>();
            mapper.AddCustomMapping(src => src.Scope, dest => dest.Scope, src => (ModuleVariableScope)src.Scope);
            mapper.AddCustomMapping(src => src, dest => dest.ViewModel,
                src => ViewModelMapping.MapViewModel(viewModels.FirstOrDefault(v => v.Id == src.ViewModelId), null),
                src => src.ViewModelId != null
            );

            var result = mapper.Map(variable);
            return result;
        }

        #endregion
    }
}