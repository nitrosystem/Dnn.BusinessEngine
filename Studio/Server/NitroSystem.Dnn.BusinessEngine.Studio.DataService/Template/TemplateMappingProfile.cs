using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Template;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Template
{
    public static class TemplateMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<TemplateInfo, TemplateViewModel>(
                (src, dest) => dest.TemplateImage = src.TemplateImage?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<TemplateInfo, TemplateViewModel>(
                (src, dest) => dest.TemplatePath = src.TemplatePath?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<TemplateInfo, TemplateViewModel>(
                (src, dest) => dest.PreviewImages = ReflectionUtil.TryJsonCasting<IEnumerable<string>>(
                    src.PreviewImages?.ReplaceFrequentTokens()));

            HybridMapper.BeforeMap<TemplateThemeInfo, TemplateThemeViewModel>(
               (src, dest) => dest.ThemeCssPath = src.ThemeCssPath?.ReplaceFrequentTokens());
        }
    }
}
