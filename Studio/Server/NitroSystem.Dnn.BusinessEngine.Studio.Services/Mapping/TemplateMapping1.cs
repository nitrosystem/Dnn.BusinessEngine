using AutoMapper;
using DotNetNuke.Collections;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.TypeCasting;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationServices.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.BusinessLogic.Mapping
{
    public static class TemplateMapping1
    {
        #region Template Mapping

        public static IEnumerable<TemplateViewModel> GetTemplatesViewModel(string moduleType, string searchText)
        {
            var templates = TemplateRepository.Instance.GetTemplates(moduleType, searchText);

            return GetTemplatesViewModel(templates, moduleType);
        }

        public static IEnumerable<TemplateViewModel> GetTemplatesViewModel(IEnumerable<TemplateInfo> templates, string moduleType)
        {
            var result = new List<TemplateViewModel>();

            foreach (var template in templates)
            {
                var templateDTO = GetTemplateViewModel(template, moduleType);
                result.Add(templateDTO);
            }

            return result;
        }

        public static TemplateViewModel GetTemplateViewModel(Guid templateID, string moduleType)
        {
            var objTemplateInfo = TemplateRepository.Instance.GetTemplate(templateID);

            return GetTemplateViewModel(objTemplateInfo, moduleType);
        }

        public static TemplateViewModel GetTemplateViewModel(TemplateInfo objTemplateInfo, string moduleType)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TemplateInfo, TemplateViewModel>()
                .ForMember(dest => dest.TemplatePath, map => map.MapFrom(source => source.TemplatePath.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions")))
                .ForMember(dest => dest.ModuleTemplates, map => map.MapFrom(source => TemplateItemRepository.Instance.GetModuleTemplate(source.Id, moduleType)))
                .ForMember(dest => dest.FieldTypes, map => map.MapFrom(source => TemplateItemRepository.Instance.GetFieldTypesTemplate(source.Id).Select(i => new TemplateFieldTypeViewModel() { FieldType = i.ItemName, Icon = "/DesktopModules/BusinessEngine/extensions/nitro-I/assets/images/field-types/checkbox.png" })))
                .ForMember(dest => dest.Themes, map => map.MapFrom(source => TemplateItemThemeRepository.Instance.GetTemplateThemes(source.Id)))
                .ForMember(dest => dest.Libraries, map => map.MapFrom(source => TemplateLibraryRepository.Instance.GetTemplateLibraries(source.Id)));
            });

            IMapper mapper = config.CreateMapper();
            var result = mapper.Map<TemplateViewModel>(objTemplateInfo);

            return result;
        }

        #endregion
    }
}