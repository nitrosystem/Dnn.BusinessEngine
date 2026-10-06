using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Extension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Extension;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Extension
{
    public static class ExtensionMappingProfile
    {
        public static void Register()
        {
            HybridMapper.AfterMap<ExtensionInfo, ExtensionViewModel>(
                (src, dest) => dest.Logo = src.Logo?.ReplaceFrequentTokens());
        }
    }
}
