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
    internal static class ModuleCustomLibraryMapping
    {
        #region Custom Library Mapping

        internal static IEnumerable<ModuleCustomLibraryViewModel> GetCustomLibrariesViewModel(Guid moduleID)
        {
            var customLibraries = ModuleCustomLibraryRepository.Instance.GetLibraries(moduleID);

            return GetCustomLibrariesViewModel(customLibraries);
        }

        internal static IEnumerable<ModuleCustomLibraryViewModel> GetCustomLibrariesViewModel(IEnumerable<ModuleCustomLibraryInfo> customLibraries)
        {
            var result = new List<ModuleCustomLibraryViewModel>();

            foreach (var objModuleCustomLibraryInfo in customLibraries ?? Enumerable.Empty<ModuleCustomLibraryInfo>())
            {
                var customLibrary = GetCustomLibraryViewModel(objModuleCustomLibraryInfo);
                result.Add(customLibrary);
            }

            return result;
        }

        internal static ModuleCustomLibraryViewModel GetCustomLibraryViewModel(ModuleCustomLibraryInfo objModuleCustomLibraryInfo)
        {
            var objLibraryInfo = LibraryRepository.Instance.GetLibrary(objModuleCustomLibraryInfo.LibraryName, objModuleCustomLibraryInfo.Version);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModuleCustomLibraryInfo, ModuleCustomLibraryViewModel>()
                .ForMember(dest => dest.LibraryID, map => map.MapFrom(source => objLibraryInfo.LibraryID))
                .ForMember(dest => dest.LocalPath, map => map.MapFrom(source => objLibraryInfo.LocalPath))
                .ForMember(dest => dest.Description, map => map.MapFrom(source => objLibraryInfo.Summary))
                .ForMember(dest => dest.Logo, map => map.MapFrom(source => objLibraryInfo.Logo))
                .ForMember(dest => dest.Resources, map => map.MapFrom(source => LibraryResourceRepository.Instance.GetResources(source.LibraryName, source.Version)));
            });

            IMapper mapper = config.CreateMapper();
            var result = mapper.Map<ModuleCustomLibraryViewModel>(objModuleCustomLibraryInfo);

            return result;
        }

        #endregion

        //#region Custom Resource

        //internal static IEnumerable<ModuleVariableViewModel> GetVariablesViewModel(Guid moduleGuid)
        //{
        //    var variables = ModuleVariableRepository.Instance.GetVariables(moduleGuid);

        //    return GetVariablesViewModel(variables);
        //}

        //internal static IEnumerable<ModuleVariableViewModel> GetVariablesViewModel(IEnumerable<ModuleVariableInfo> variables)
        //{
        //    var result = new List<ModuleVariableViewModel>();
        //    var customLibraries = ModuleCustomLibraryRepository.Instance.GetCustomLibraries();

        //    if (variables != null)
        //    {
        //        foreach (var objModuleVariableInfo in variables)
        //        {
        //            var variable = GetVariableViewModel(objModuleVariableInfo, customLibraries.FirstOrDefault(v => v.CustomLibrary == objModuleVariableInfo.CustomLibrary));
        //            result.Add(variable);
        //        }
        //    }

        //    return result;
        //}

        //internal static ModuleVariableViewModel GetVariableViewModel(ModuleVariableInfo objModuleVariableInfo, ModuleCustomLibraryInfo objModuleCustomLibraryInfo = null)
        //{
        //    if (objModuleCustomLibraryInfo == null) objModuleCustomLibraryInfo = ModuleCustomLibraryRepository.Instance.GetCustomLibraries().FirstOrDefault(v => v.CustomLibrary == objModuleVariableInfo.CustomLibrary);

        //    var config = new MapperConfiguration(cfg =>
        //    {
        //        cfg.CreateMap<ModuleVariableInfo, ModuleVariableViewModel>()
        //        .ForMember(dest => dest.ScopeName, map => map.MapFrom(source => source.Scope.ToScopeString()))
        //        .ForMember(dest => dest.CustomLibraryLanguageName, map => map.MapFrom(source => objModuleCustomLibraryInfo.Language.ToLanguageString()))
        //        .ForMember(dest => dest.CustomLibraryCategory, map => map.MapFrom(source => objModuleCustomLibraryInfo.Category))
        //        .ForMember(dest => dest.CustomLibraryCategoryIcon, map => map.MapFrom(source => objModuleCustomLibraryInfo.Category.ToCateogoryIcon()))
        //        .ForMember(dest => dest.CustomLibraryIcon, map => map.MapFrom(source => objModuleCustomLibraryInfo.Icon.Replace("[MODULEPATH]", "/DesktopModules/BusinessEngine")))
        //        .ForMember(dest => dest.ViewModel, map => { map.PreCondition(source => (source.ViewModelID != null)); map.MapFrom(source => ViewModelMapping.GetViewModelViewModel(source.ViewModelID.Value)); });
        //    });

        //    IMapper mapper = config.CreateMapper();
        //    var result = mapper.Map<ModuleVariableViewModel>(objModuleVariableInfo);

        //    return result;
        //}

        //#endregion
    }
}