using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.DatabaseEntities.Tables;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.Services
{
    public class ServiceMappingProfile
    {
        public static void Register()
        {
            #region Bind Entity Service

            HybridMapper.AfterMap<BindEntityServiceInfo, BindEntityServiceViewModel>(
                (src, dest) => dest.ModelProperties = ReflectionUtil.TryJsonCasting<IEnumerable<ModelPropertyViewModel>>(src.ModelProperties));

            HybridMapper.AfterMap<BindEntityServiceInfo, BindEntityServiceViewModel>(
                (src, dest) => dest.Filters = ReflectionUtil.TryJsonCasting<IEnumerable<FilterItemViewModel>>(src.Filters));

            HybridMapper.AfterMap<BindEntityServiceInfo, BindEntityServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.AfterMap<BindEntityServiceViewModel, BindEntityServiceInfo>(
                (src, dest) => dest.ModelProperties = JsonConvert.SerializeObject(src.ModelProperties));

            HybridMapper.AfterMap<BindEntityServiceViewModel, BindEntityServiceInfo>(
                (src, dest) => dest.Filters = JsonConvert.SerializeObject(src.Filters));

            HybridMapper.AfterMap<BindEntityServiceViewModel, BindEntityServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion
           
            #region Data Row Service

            HybridMapper.AfterMap<DataRowServiceInfo, DataRowServiceViewModel>(
                (src, dest) => dest.Entities = ReflectionUtil.TryJsonCasting<IEnumerable<EntityViewModel>>(src.Entities));

            HybridMapper.AfterMap<DataRowServiceInfo, DataRowServiceViewModel>(
                (src, dest) => dest.JoinRelationships = ReflectionUtil.TryJsonCasting<IEnumerable<EntityJoinRelationViewModel>>(src.JoinRelationships));

            HybridMapper.AfterMap<DataRowServiceInfo, DataRowServiceViewModel>(
                (src, dest) => dest.ModelProperties = ReflectionUtil.TryJsonCasting<IEnumerable<ModelPropertyViewModel>>(src.ModelProperties));

            HybridMapper.AfterMap<DataRowServiceInfo, DataRowServiceViewModel>(
                (src, dest) => dest.Filters = ReflectionUtil.TryJsonCasting<IEnumerable<FilterItemViewModel>>(src.Filters));

            HybridMapper.AfterMap<DataRowServiceInfo, DataRowServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.AfterMap<DataRowServiceViewModel, DataRowServiceInfo>(
                (src, dest) => dest.Entities = JsonConvert.SerializeObject(src.Entities));

            HybridMapper.AfterMap<DataRowServiceViewModel, DataRowServiceInfo>(
                (src, dest) => dest.JoinRelationships = JsonConvert.SerializeObject(src.JoinRelationships));

            HybridMapper.AfterMap<DataRowServiceViewModel, DataRowServiceInfo>(
                (src, dest) => dest.ModelProperties = JsonConvert.SerializeObject(src.ModelProperties));

            HybridMapper.AfterMap<DataRowServiceViewModel, DataRowServiceInfo>(
                (src, dest) => dest.Filters = JsonConvert.SerializeObject(src.Filters));

            HybridMapper.AfterMap<DataRowServiceViewModel, DataRowServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion

            #region Data Source Service

            HybridMapper.AfterMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.Entities = ReflectionUtil.TryJsonCasting<IEnumerable<EntityViewModel>>(src.Entities));

            HybridMapper.AfterMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.JoinRelationships = ReflectionUtil.TryJsonCasting<IEnumerable<EntityJoinRelationViewModel>>(src.JoinRelationships));

            HybridMapper.AfterMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.ModelProperties = ReflectionUtil.TryJsonCasting<IEnumerable<ModelPropertyViewModel>>(src.ModelProperties));

            HybridMapper.AfterMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.Filters = ReflectionUtil.TryJsonCasting<IEnumerable<FilterItemViewModel>>(src.Filters));

            HybridMapper.AfterMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.SortItems = ReflectionUtil.TryJsonCasting<IEnumerable<SortItemViewModel>>(src.SortItems));

            HybridMapper.AfterMap<DataSourceServiceInfo, DataSourceServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.AfterMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.Entities = JsonConvert.SerializeObject(src.Entities));

            HybridMapper.AfterMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.JoinRelationships = JsonConvert.SerializeObject(src.JoinRelationships));

            HybridMapper.AfterMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.ModelProperties = JsonConvert.SerializeObject(src.ModelProperties));

            HybridMapper.AfterMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.Filters = JsonConvert.SerializeObject(src.Filters));

            HybridMapper.AfterMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.SortItems = JsonConvert.SerializeObject(src.SortItems));

            HybridMapper.AfterMap<DataSourceServiceViewModel, DataSourceServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion

            #region Submit Entity Service

            HybridMapper.AfterMap<SubmitEntityServiceInfo, SubmitEntityServiceViewModel>(
                (src, dest) => dest.ActionType = (SubmitEntityActionType)src.ActionType);

            HybridMapper.AfterMap<SubmitEntityServiceInfo, SubmitEntityServiceViewModel>(
                (src, dest) => dest.Entity = ReflectionUtil.TryJsonCasting<ViewModels.SubmitEntity.EntityViewModel>(src.Entity));

            HybridMapper.AfterMap<SubmitEntityServiceInfo, SubmitEntityServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.AfterMap<SubmitEntityServiceViewModel, SubmitEntityServiceInfo>(
                (src, dest) => dest.ActionType = (int)src.ActionType);

            HybridMapper.AfterMap<SubmitEntityServiceViewModel, SubmitEntityServiceInfo>(
                (src, dest) => dest.Entity = JsonConvert.SerializeObject(src.Entity));

            HybridMapper.AfterMap<SubmitEntityServiceViewModel, SubmitEntityServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion

            #region Delete Entity Row Service

            HybridMapper.AfterMap<DeleteEntityRowServiceInfo, DeleteEntityRowServiceViewModel>(
                (src, dest) => dest.Conditions = ReflectionUtil.TryJsonCasting<IEnumerable<FilterItemViewModel>>(src.Conditions));

            HybridMapper.AfterMap<DeleteEntityRowServiceInfo, DeleteEntityRowServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.AfterMap<DeleteEntityRowServiceViewModel, DeleteEntityRowServiceInfo>(
                (src, dest) => dest.Conditions = JsonConvert.SerializeObject(src.Conditions));

            HybridMapper.AfterMap<DeleteEntityRowServiceViewModel, DeleteEntityRowServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion

            #region Custom Query Service

            HybridMapper.AfterMap<CustomQueryServiceInfo, CustomQueryServiceViewModel>(
                (src, dest) => dest.ResultType = (ResultType)src.ResultType);

            HybridMapper.AfterMap<CustomQueryServiceInfo, CustomQueryServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.AfterMap<CustomQueryServiceViewModel, CustomQueryServiceInfo>(
                (src, dest) => dest.ResultType = (int)src.ResultType);

            HybridMapper.AfterMap<CustomQueryServiceViewModel, CustomQueryServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion
        }
    }
}
