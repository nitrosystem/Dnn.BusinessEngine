using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Library;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Base
{
    public class BaseMappingProfile
    {
        public static void Register()
        {
            HybridMapper.AfterMap<LibraryInfo, LibraryListItem>(
               (src, dest) => dest.Logo = src.Logo?.ReplaceFrequentTokens());

            HybridMapper.AfterMap<LibraryResourceInfo, LibraryResourceListItem>(
               (src, dest) => dest.ResourceContentType = (ResourceContentType)src.ResourceContentType);

            HybridMapper.AfterMap<LibraryResourceInfo, LibraryResourceListItem>(
               (src, dest) => dest.ResourcePath = src.ResourcePath?.ReplaceFrequentTokens());
        }
    }
}
