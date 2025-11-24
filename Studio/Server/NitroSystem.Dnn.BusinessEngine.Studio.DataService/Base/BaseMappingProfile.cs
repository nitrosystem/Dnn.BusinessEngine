using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Base
{
    public class BaseMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<EntityInfo, EntityViewModel>(
               (src, dest) => dest.EntityType = (EntityType)src.EntityType);

            HybridMapper.BeforeMap<EntityInfo, EntityViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.BeforeMap<EntityViewModel, EntityInfo>(
               (src, dest) => dest.EntityType = (int)src.EntityType);

            HybridMapper.BeforeMap<EntityViewModel, EntityInfo>(
                    (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));
        }
    }
}
