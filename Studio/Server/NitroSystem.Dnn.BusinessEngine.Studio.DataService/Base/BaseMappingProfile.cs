using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Base
{
    public class BaseMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<LibraryResourceInfo, LibraryResourceListItem>(
               (src, dest) => dest.ResourceContentType = (ResourceContentType)src.ResourceContentType);
        }
    }
}
