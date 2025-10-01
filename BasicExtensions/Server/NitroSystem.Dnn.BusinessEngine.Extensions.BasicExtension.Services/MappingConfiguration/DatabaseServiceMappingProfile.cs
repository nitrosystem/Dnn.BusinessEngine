using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Database;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.StudioServices;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ListItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.MappingConfiguration
{
    public static class DatabaseServiceMappingProfile
    {
        public static void Register()
        {
            #region Bind Entity Service

            HybridMapper.BeforeMap<BindEntityServiceInfo, BindEntityServiceViewModel>(
                (src, dest) => dest.ModelProperties = ReflectionUtil.TryJsonCasting<IEnumerable<ModelPropertyInfo>>(src.ModelProperties));

            HybridMapper.BeforeMap<BindEntityServiceInfo, BindEntityServiceViewModel>(
                (src, dest) => dest.Filters = ReflectionUtil.TryJsonCasting<IEnumerable<FilterItemInfo>>(src.Filters));

            HybridMapper.BeforeMap<BindEntityServiceInfo, BindEntityServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<BindEntityServiceViewModel, BindEntityServiceInfo>(
                (src, dest) => dest.ModelProperties = JsonConvert.SerializeObject(src.ModelProperties));

            HybridMapper.BeforeMap<BindEntityServiceViewModel, BindEntityServiceInfo>(
                (src, dest) => dest.Filters = JsonConvert.SerializeObject(src.Filters));

            HybridMapper.BeforeMap<BindEntityServiceViewModel, BindEntityServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion

            #region Custom Query Service

            HybridMapper.BeforeMap<CustomQueryServiceInfo, CustomQueryServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<CustomQueryServiceViewModel, CustomQueryServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion

            #region Data Row Service

            HybridMapper.BeforeMap<DataRowServiceInfo, DataRowServiceViewModel>(
                (src, dest) => dest.Entities = ReflectionUtil.TryJsonCasting<IEnumerable<EntityInfo>>(src.Entities));

            HybridMapper.BeforeMap<DataRowServiceInfo, DataRowServiceViewModel>(
                (src, dest) => dest.JoinRelationships = ReflectionUtil.TryJsonCasting<IEnumerable<EntityJoinRelationInfo>>(src.JoinRelationships));

            HybridMapper.BeforeMap<DataRowServiceInfo, DataRowServiceViewModel>(
                (src, dest) => dest.ModelProperties = ReflectionUtil.TryJsonCasting<IEnumerable<ModelPropertyInfo>>(src.ModelProperties));

            HybridMapper.BeforeMap<DataRowServiceInfo, DataRowServiceViewModel>(
                (src, dest) => dest.Filters = ReflectionUtil.TryJsonCasting<IEnumerable<FilterItemInfo>>(src.Filters));

            HybridMapper.BeforeMap<DataRowServiceInfo, DataRowServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            ///////////////////////////////////////////////////////////////////////////////////////////////

            HybridMapper.BeforeMap<DataRowServiceViewModel, DataRowServiceInfo>(
                (src, dest) => dest.Entities = JsonConvert.SerializeObject(src.Entities));

            HybridMapper.BeforeMap<DataRowServiceViewModel, DataRowServiceInfo>(
                (src, dest) => dest.JoinRelationships = JsonConvert.SerializeObject(src.JoinRelationships));

            HybridMapper.BeforeMap<DataRowServiceViewModel, DataRowServiceInfo>(
                (src, dest) => dest.ModelProperties = JsonConvert.SerializeObject(src.ModelProperties));

            HybridMapper.BeforeMap<DataRowServiceViewModel, DataRowServiceInfo>(
                (src, dest) => dest.Filters = JsonConvert.SerializeObject(src.Filters));

            HybridMapper.BeforeMap<DataRowServiceViewModel, DataRowServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion

            #region Data Source Service

            HybridMapper.BeforeMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.Entities = ReflectionUtil.TryJsonCasting<IEnumerable<EntityInfo>>(src.Entities));

            HybridMapper.BeforeMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.JoinRelationships = ReflectionUtil.TryJsonCasting<IEnumerable<EntityJoinRelationInfo>>(src.JoinRelationships));

            HybridMapper.BeforeMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.ModelProperties = ReflectionUtil.TryJsonCasting<IEnumerable<ModelPropertyInfo>>(src.ModelProperties));

            HybridMapper.BeforeMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.Filters = ReflectionUtil.TryJsonCasting<IEnumerable<FilterItemInfo>>(src.Filters));

            HybridMapper.BeforeMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.SortItems = ReflectionUtil.TryJsonCasting<IEnumerable<SortItemInfo>>(src.SortItems));

            HybridMapper.BeforeMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            ///////////////////////////////////////////////////////////////////////////////////////////////

            HybridMapper.BeforeMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.Entities = JsonConvert.SerializeObject(src.Entities));

            HybridMapper.BeforeMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.JoinRelationships = JsonConvert.SerializeObject(src.JoinRelationships));

            HybridMapper.BeforeMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.ModelProperties = JsonConvert.SerializeObject(src.ModelProperties));

            HybridMapper.BeforeMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.Filters = JsonConvert.SerializeObject(src.Filters));

            HybridMapper.BeforeMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.SortItems = JsonConvert.SerializeObject(src.SortItems));

            HybridMapper.BeforeMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion

            #region Delete Entity Row Service

            HybridMapper.BeforeMap<DeleteEntityRowServiceInfo, DeleteEntityRowServiceViewModel>(
                (src, dest) => dest.Conditions = ReflectionUtil.TryJsonCasting<IEnumerable<FilterItemInfo>>(src.Conditions));

            HybridMapper.BeforeMap<DeleteEntityRowServiceInfo, DeleteEntityRowServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<DeleteEntityRowServiceViewModel, DeleteEntityRowServiceInfo>(
                (src, dest) => dest.Conditions = JsonConvert.SerializeObject(src.Conditions));

            HybridMapper.BeforeMap<DeleteEntityRowServiceViewModel, DeleteEntityRowServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion

            #region Submit Entity Service

            HybridMapper.BeforeMap<SubmitEntityServiceInfo, SubmitEntityServiceViewModel>(
                (src, dest) => dest.ActionType = (ActionType)src.ActionType);

            HybridMapper.BeforeMap<SubmitEntityServiceInfo, SubmitEntityServiceViewModel>(
                (src, dest) => dest.Entity = ReflectionUtil.TryJsonCasting<Models.Database.SubmitEntity.EntityInfo>(src.Entity));

            HybridMapper.BeforeMap<SubmitEntityServiceInfo, SubmitEntityServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<SubmitEntityServiceViewModel, SubmitEntityServiceInfo>(
                (src, dest) => dest.ActionType = (int)src.ActionType);

            HybridMapper.BeforeMap<SubmitEntityServiceViewModel, SubmitEntityServiceInfo>(
                (src, dest) => dest.Entity = JsonConvert.SerializeObject(src.Entity));

            HybridMapper.BeforeMap<SubmitEntityServiceViewModel, SubmitEntityServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion
        }
    }
}
