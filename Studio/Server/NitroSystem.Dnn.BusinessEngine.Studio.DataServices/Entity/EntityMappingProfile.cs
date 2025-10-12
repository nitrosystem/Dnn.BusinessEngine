using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Entity
{
    public static class EntityMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<EntityInfo, EntityViewModel>(
                (src, dest) => dest.EntityType = (EntityType)src.EntityType);

            HybridMapper.BeforeMap<EntityInfo, EntityViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<EntityViewModel, EntityInfo>(
                (src, dest) => dest.EntityType = (int)src.EntityType);

            HybridMapper.BeforeMap<EntityViewModel, EntityInfo>(
                    (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));
        }
    }
}
