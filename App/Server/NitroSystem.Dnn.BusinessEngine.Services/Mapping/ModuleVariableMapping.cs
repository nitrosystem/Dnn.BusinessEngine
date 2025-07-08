using AutoMapper;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.BusinessLogic.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Common.TypeCasting;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Enums;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.BusinessLogic.Mapping
{
    public static class ModuleVariableMapping
    {
        #region Variable Type Mapping

        public static IEnumerable<VariableTypeViewModel> GetVariableTypesViewModel()
        {
            var variableTypes = VariableTypeRepository.Instance.GetVariableTypes();

            return GetVariableTypesViewModel(variableTypes);
        }

        public static IEnumerable<VariableTypeViewModel> GetVariableTypesViewModel(IEnumerable<VariableTypeInfo> variableTypes)
        {
            var result = new List<VariableTypeViewModel>();

            foreach (var objVariableTypeInfo in variableTypes ?? Enumerable.Empty<VariableTypeInfo>())
            {
                var variableType = GetVariableTypeInfo(objVariableTypeInfo);
                result.Add(variableType);
            }

            return result;
        }

        public static VariableTypeViewModel GetVariableTypeInfo(VariableTypeInfo objVariableTypeInfo)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VariableTypeInfo, VariableTypeViewModel>()
                .ForMember(dest => dest.Icon, map => map.MapFrom(source => source.Icon.Replace("[MODULEPATH]", "/DesktopModules/BusinessEngine")))
                .ForMember(dest => dest.CategoryIcon, map => map.MapFrom(source => source.Category.ToCateogoryIcon()))
                .ForMember(dest => dest.LanguageName, map => map.MapFrom(source => source.Language.ToLanguageString()));
            });

            IMapper mapper = config.CreateMapper();
            var result = mapper.Map<VariableTypeViewModel>(objVariableTypeInfo);

            return result;
        }

        public static string ToLanguageString(this VariableTypeLanguage language)
        {
            // TODO: validation
            switch (language)
            {
                case VariableTypeLanguage.Csharp:
                    return "Csharp";
                case VariableTypeLanguage.SqlServer:
                    return "Sql Server";
                case VariableTypeLanguage.JavaScript:
                    return "Java Script";
                default:
                    return string.Empty;
            }
        }

        public static string ToScopeString(this ModuleVariableScope scope)
        {
            switch (scope)
            {
                case ModuleVariableScope.Global:
                    return "Global";
                case ModuleVariableScope.ClientSide:
                    return "ClientSide";
                case ModuleVariableScope.ServerSide:
                    return "ServerSide";
                default:
                    return string.Empty;
            }
        }

        public static string ToCateogoryIcon(this string variableCategory)
        {
            switch (variableCategory)
            {
                case "Character":
                    return "replace-all";
                case "Numeric":
                    return "symbol-operator";
                case "Conditional":
                    return "type-hierarchy-sub";
                case "Objective":
                    return "extensions";
                case "Array":
                    return "surround-with";
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Variable Mapping

        public static IEnumerable<ModuleVariableViewModel> GetScenarioVariablesViewModel(Guid scenarioID)
        {
            var variables = ModuleVariableRepository.Instance.GetScenarioVariables(scenarioID);

            return GetVariablesViewModel(variables);
        }

        public static IEnumerable<ModuleVariableViewModel> GetVariablesViewModel(Guid moduleGuid)
        {
            var variables = ModuleVariableRepository.Instance.GetVariables(moduleGuid);

            return GetVariablesViewModel(variables);
        }

        public static IEnumerable<ModuleVariableViewModel> GetVariablesViewModel(IEnumerable<ModuleVariableInfo> variables)
        {
            var result = new List<ModuleVariableViewModel>();
            var variableTypes = VariableTypeRepository.Instance.GetVariableTypes();

            if (variables != null)
            {
                foreach (var objModuleVariableInfo in variables)
                {
                    var variable = GetVariableViewModel(objModuleVariableInfo, variableTypes.FirstOrDefault(v => v.VariableType == objModuleVariableInfo.VariableType));
                    result.Add(variable);
                }
            }

            return result;
        }

        public static ModuleVariableViewModel GetVariableViewModel(ModuleVariableInfo objModuleVariableInfo, VariableTypeInfo objVariableTypeInfo = null)
        {
            if (objVariableTypeInfo == null) objVariableTypeInfo = VariableTypeRepository.Instance.GetVariableTypes().FirstOrDefault(v => v.VariableType == objModuleVariableInfo.VariableType);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModuleVariableInfo, ModuleVariableViewModel>()
                .ForMember(dest => dest.ScopeName, map => map.MapFrom(source => source.Scope.ToScopeString()))
                .ForMember(dest => dest.VariableTypeLanguageName, map => map.MapFrom(source => objVariableTypeInfo.Language.ToLanguageString()))
                .ForMember(dest => dest.VariableTypeCategory, map => map.MapFrom(source => objVariableTypeInfo.Category))
                .ForMember(dest => dest.VariableTypeCategoryIcon, map => map.MapFrom(source => objVariableTypeInfo.Category.ToCateogoryIcon()))
                .ForMember(dest => dest.VariableTypeIcon, map => map.MapFrom(source => objVariableTypeInfo.Icon.Replace("[MODULEPATH]", "/DesktopModules/BusinessEngine")))
                .ForMember(dest => dest.ViewModel, map => { map.PreCondition(source => (source.ViewModelID != null)); map.MapFrom(source => ViewModelMapping.GetViewModelViewModel(source.ViewModelID.Value)); });
            });

            IMapper mapper = config.CreateMapper();
            var result = mapper.Map<ModuleVariableViewModel>(objModuleVariableInfo);

            return result;
        }

        #endregion
    }
}