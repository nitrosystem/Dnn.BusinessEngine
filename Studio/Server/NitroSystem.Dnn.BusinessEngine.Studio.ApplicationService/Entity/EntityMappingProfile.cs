using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Entity
{
    public static class EntityMappingProfile
    {
        public static void Register()
        {
            HybridMapper.AfterMap<EntityInfo, EntityViewModel>(
                (src, dest) => dest.EntityType = (EntityType)src.EntityType);

            HybridMapper.AfterMap<EntityInfo, EntityViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.AfterMap<EntityViewModel, EntityInfo>(
                (src, dest) => dest.EntityType = (int)src.EntityType);

            HybridMapper.AfterMap<EntityViewModel, EntityInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            HybridMapper.AfterMap<EntityInfo, EntityListItem>(
                (src, dest) => dest.EntityType = (EntityType)src.EntityType);
        }
    }
}
