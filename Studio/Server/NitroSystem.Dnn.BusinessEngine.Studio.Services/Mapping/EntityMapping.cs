using DotNetNuke.Collections;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Shared.Reflection;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping
{
    public class EntityMapping
    {
        #region Entity Mapping

        public static IEnumerable<EntityViewModel> MapEntitiesViewModel(IEnumerable<EntityInfo> entities, IEnumerable<EntityColumnInfo> entityColumns)
        {
            var columns = BaseMapping<EntityColumnInfo, EntityColumnViewModel>.MapViewModels(entityColumns);

            var itemsDict = columns.GroupBy(c => c.EntityId)
                                     .ToDictionary(g => g.Key, g => g.AsEnumerable());

            return entities.Select(entity =>
            {
                var items = itemsDict.TryGetValue(entity.Id, out var cols) ? cols : Enumerable.Empty<EntityColumnViewModel>();
                return MapEntityViewModel(entity, items);
            });
        }

        public static EntityViewModel MapEntityViewModel(EntityInfo objEntityInfo, IEnumerable<EntityColumnViewModel> columns)
        {
            if (objEntityInfo == null) return null;

            var mapper = new ExpressionMapper<EntityInfo, EntityViewModel>();
            mapper.AddCustomMapping(src => src.EntityType, dest => dest.EntityType, src => (EntityType)src.EntityType);
            mapper.AddCustomMapping(src => src.Settings, dest => dest.Settings, src => TypeCasting.TryJsonCasting<IDictionary<string, object>>(src.Settings));
            mapper.AddCustomMapping(src => src, dest => dest.Columns, map => columns);

            return mapper.Map(objEntityInfo);
        }

        public static EntityInfo MapEntityInfo(EntityViewModel entity)
        {
            if (entity == null) return null;

            var mapper = new ExpressionMapper<EntityViewModel, EntityInfo>();
            mapper.AddCustomMapping(src => src.EntityType, dest => dest.EntityType, src => (int)src.EntityType);
            mapper.AddCustomMapping(src => src.Settings, dest => dest.Settings, src => JsonConvert.SerializeObject(src.Settings));
            return mapper.Map(entity);
        }

        #endregion
    }
}