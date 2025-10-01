using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Template;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeGeneration;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.MappingConfiguration
{
    public static class AppModelMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<AppModelPropertyInfo, PropertyDefinition>(
                (src, dest) => dest.Name = src.PropertyName);

            HybridMapper.BeforeMap<AppModelPropertyInfo, PropertyDefinition>(
                (src, dest) => dest.ClrType = src.PropertyType);
        }
    }
}
