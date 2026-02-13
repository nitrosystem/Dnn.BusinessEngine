using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.AppModel;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.AppModel
{
    public static class AppModelMappingProfile
    {
        public static void Register()
        {
            #region App Model

            HybridMapper.BeforeMap<AppModelInfo, AppModelViewModel>(
                (src, dest) => dest.ModelType = (AppModelType)src.ModelType);

            HybridMapper.BeforeMap<AppModelInfo, AppModelViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.BeforeMap<AppModelViewModel, AppModelInfo>(
                (src, dest) => dest.ModelType = (int)src.ModelType);

            HybridMapper.BeforeMap<AppModelViewModel, AppModelInfo>(
                    (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            HybridMapper.BeforeMap<AppModelInfo, AppModelListItem>(
                (src, dest) => dest.ModelType = (AppModelType)src.ModelType);

            #endregion

            #region App Model Properties

            HybridMapper.BeforeMap<AppModelPropertyInfo, AppModelPropertyViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.BeforeMap<AppModelPropertyViewModel, AppModelPropertyInfo>(
                    (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            HybridMapper.BeforeMap<AppModelPropertyInfo, PropertyDefinition>(
                (src, dest) => dest.Name = src.PropertyName);

            HybridMapper.BeforeMap<AppModelPropertyInfo, PropertyDefinition>(
                (src, dest) => dest.ClrType = src.PropertyType);

            HybridMapper.BeforeMap<AppModelPropertyViewModel, PropertyDefinition>(
                (src, dest) => dest.Name = src.PropertyName);

            HybridMapper.BeforeMap<AppModelPropertyViewModel, PropertyDefinition>(
                (src, dest) => dest.ClrType = src.PropertyType);

            #endregion
        }
    }
}
