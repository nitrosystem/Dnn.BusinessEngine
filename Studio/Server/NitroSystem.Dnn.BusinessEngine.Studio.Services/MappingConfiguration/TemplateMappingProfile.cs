using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Template;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.MappingConfiguration
{
    public static class TemplateMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<TemplateInfo, TemplateViewModel>(
                (src, dest) => dest.TemplateImage = dest.TemplateImage?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<TemplateInfo, TemplateViewModel>(
                (src, dest) => dest.TemplatePath = dest.TemplatePath?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<TemplateInfo, TemplateViewModel>(
                (src, dest) => dest.PreviewImages = dest.PreviewImages?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<TemplateThemeInfo, TemplateThemeViewModel>(
               (src, dest) => dest.ThemeCssPath = dest.ThemeCssPath?.ReplaceFrequentTokens());
        }
    }
}
