using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Extension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Provider;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Provider
{
    public class ProviderMappingProfile
    {
        public static void Register()
        {
            HybridMapper.AfterMap<ProviderInfo, ProviderListItem>(
               (src, dest) => dest.ProviderType = (ProviderType)src.ProviderType);

            HybridMapper.AfterMap<ProviderInfo, ProviderViewModel>(
               (src, dest) => dest.ProviderType = (ProviderType)src.ProviderType);
        }
    }
}
