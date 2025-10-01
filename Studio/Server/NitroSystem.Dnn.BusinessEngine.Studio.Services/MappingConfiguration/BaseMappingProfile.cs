using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.MappingConfiguration
{
    public class BaseMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<ModuleInfo, ModuleViewModel>(
                (src, dest) => dest.ModuleType = (ModuleType)src.ModuleType);
        }
    }
}
