using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.MappingConfiguration
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
