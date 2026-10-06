using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.AppModel;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.AppModel
{
    public static class AppModelMappingProfile
    {
        public static void Register()
        {
            #region App Model

            HybridMapper.AfterMap<AppModelInfo, AppModelViewModel>(
                (src, dest) => dest.ModelType = (AppModelType)src.ModelType);

            HybridMapper.AfterMap<AppModelInfo, AppModelViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.AfterMap<AppModelViewModel, AppModelInfo>(
                (src, dest) => dest.ModelType = (int)src.ModelType);

            HybridMapper.AfterMap<AppModelViewModel, AppModelInfo>(
                    (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            HybridMapper.AfterMap<AppModelInfo, AppModelListItem>(
                (src, dest) => dest.ModelType = (AppModelType)src.ModelType);

            #endregion

            #region App Model Properties

            HybridMapper.AfterMap<AppModelPropertyInfo, AppModelPropertyViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.AfterMap<AppModelPropertyViewModel, AppModelPropertyInfo>(
                    (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            HybridMapper.AfterMap<AppModelPropertyInfo, PropertyDefinition>(
                (src, dest) => dest.Name = src.PropertyName);

            HybridMapper.AfterMap<AppModelPropertyInfo, PropertyDefinition>(
                (src, dest) => dest.ClrType = src.PropertyType);

            HybridMapper.AfterMap<AppModelPropertyViewModel, PropertyDefinition>(
                (src, dest) => dest.Name = src.PropertyName);

            HybridMapper.AfterMap<AppModelPropertyViewModel, PropertyDefinition>(
                (src, dest) => dest.ClrType = src.PropertyType);

            #endregion
        }
    }
}
