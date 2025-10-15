using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeGeneration;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.AppModel;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.AppModel
{
    public static class AppModelMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<AppModelInfo, AppModelViewModel>(
                (src, dest) => dest.ModelType = (AppModelType)src.ModelType);

            HybridMapper.BeforeMap<AppModelViewModel, AppModelInfo>(
                (src, dest) => dest.ModelType = (int)src.ModelType);

            HybridMapper.BeforeMap<AppModelPropertyViewModel, PropertyDefinition>(
                (src, dest) => dest.Name = src.PropertyName);

            HybridMapper.BeforeMap<AppModelPropertyViewModel, PropertyDefinition>(
                (src, dest) => dest.ClrType = src.PropertyType);
        }
    }
}
