using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Template;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Template;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Template
{
    public static class TemplateMappingProfile
    {
        public static void Register()
        {
            HybridMapper.AfterMap<TemplateInfo, TemplateViewModel>(
                (src, dest) => dest.TemplateImage = src.TemplateImage?.ReplaceFrequentTokens());

            HybridMapper.AfterMap<TemplateInfo, TemplateViewModel>(
                (src, dest) => dest.TemplatePath = src.TemplatePath?.ReplaceFrequentTokens());

            HybridMapper.AfterMap<TemplateInfo, TemplateViewModel>(
                (src, dest) => dest.TemplateCssPath = src.TemplateCssPath?.ReplaceFrequentTokens());

            HybridMapper.AfterMap<TemplateInfo, TemplateViewModel>(
                (src, dest) => dest.PreviewImages = ReflectionUtil.TryJsonCasting<IEnumerable<string>>(
                    src.PreviewImages?.ReplaceFrequentTokens()));

            HybridMapper.AfterMap<TemplateThemeInfo, TemplateThemeViewModel>(
               (src, dest) => dest.ThemeCssPath = src.ThemeCssPath?.ReplaceFrequentTokens());
        }
    }
}
