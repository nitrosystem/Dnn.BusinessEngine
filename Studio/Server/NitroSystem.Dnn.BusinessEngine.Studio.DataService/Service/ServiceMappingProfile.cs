using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using System.Linq;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Service
{
    public class ServiceMappingProfile
    {
        public static void Register()
        {
            #region Service Type

            HybridMapper.BeforeMap<ServiceTypeView, ServiceTypeListItem>(
                (src, dest) => dest.Icon = src.Icon?.ReplaceFrequentTokens());

            #endregion

            #region Service

            HybridMapper.BeforeMap<ServiceInfo, ServiceViewModel>(
                (src, dest) => dest.ServiceTypeIcon = dest.ServiceTypeIcon?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<ServiceInfo, ServiceViewModel>(
                (src, dest) => dest.CacheOperation = (CacheOperation)src.CacheOperation);

            HybridMapper.BeforeMap<ServiceInfo, ServiceViewModel>(
                (src, dest) => dest.CacheKeys = src.CacheKey?.Split(','));

            HybridMapper.BeforeMap<ServiceInfo, ServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.BeforeMap<ServiceView, ServiceViewModel>(
                (src, dest) => dest.ServiceTypeIcon = dest.ServiceTypeIcon?.ReplaceFrequentTokens());

            HybridMapper.BeforeMap<ServiceView, ServiceViewModel>(
                (src, dest) => dest.CacheOperation = (CacheOperation)src.CacheOperation);

            HybridMapper.BeforeMap<ServiceView, ServiceViewModel>(
                (src, dest) => dest.CacheKeys = src.CacheKey?.Split(','));

            HybridMapper.BeforeMap<ServiceView, ServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.BeforeMap<ServiceViewModel, ServiceInfo>(
                (src, dest) => dest.CacheOperation = (int)src.CacheOperation);

            HybridMapper.BeforeMap<ServiceViewModel, ServiceInfo>(
                (src, dest) => dest.CacheKey = src.CacheOperation == CacheOperation.ClearCache
                    ? string.Join(",", src.CacheKeys ?? Enumerable.Empty<string>())
                    : src.CacheKey
                );

            HybridMapper.BeforeMap<ServiceViewModel, ServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion
        }
    }
}
