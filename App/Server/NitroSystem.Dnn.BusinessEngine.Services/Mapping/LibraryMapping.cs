using AutoMapper;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.BusinessLogic.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Common.TypeCasting;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.BusinessLogic.Mapping
{
    public static class LibraryMapping
    {
        #region Library Mapping

        public static IEnumerable<LibraryViewModel> GetLibrariesViewModel()
        {
            var libraries = LibraryRepository.Instance.GetLibraries();

            return GetLibrariesViewModel(libraries);
        }

        public static IEnumerable<LibraryViewModel> GetLibrariesViewModel(Guid scenarioID)
        {
            var libraries = LibraryRepository.Instance.GetLibraries();

            return GetLibrariesViewModel(libraries);
        }

        public static IEnumerable<LibraryViewModel> GetLibrariesViewModel(IEnumerable<LibraryInfo> libraries)
        {
            var result = new List<LibraryViewModel>();

            if (libraries != null)
            {
                foreach (var library in libraries)
                {
                    var libraryDTO = GetLibraryViewModel(library);
                    result.Add(libraryDTO);
                }
            }

            return result;
        }

        public static LibraryViewModel GetLibraryViewModel(Guid libraryID)
        {
            var objLibraryInfo = LibraryRepository.Instance.GetLibrary(libraryID);

            return GetLibraryViewModel(objLibraryInfo);
        }

        public static LibraryViewModel GetLibraryViewModel(LibraryInfo objLibraryInfo)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LibraryInfo, LibraryViewModel>()
                .ForMember(dest => dest.Resources, map => map.MapFrom(source => LibraryResourceRepository.Instance.GetResources(source.LibraryID)));
            });

            IMapper mapper = config.CreateMapper();
            var result = mapper.Map<LibraryViewModel>(objLibraryInfo);

            return result;
        }

        #endregion
    }
}