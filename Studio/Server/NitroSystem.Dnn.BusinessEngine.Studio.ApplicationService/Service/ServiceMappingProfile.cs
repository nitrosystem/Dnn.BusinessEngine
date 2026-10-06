using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Service
{
    public class ServiceMappingProfile
    {
        public static void Register()
        {
            #region Service Type

            HybridMapper.AfterMap<ServiceTypeInfo, ServiceTypeListItem>(
                (src, dest) => dest.Icon = src.Icon?.ReplaceFrequentTokens());

            #endregion

            #region Service

            HybridMapper.AfterMap<ServiceInfo, ServiceViewModel>(
                (src, dest) => dest.ServiceTypeIcon = dest.ServiceTypeIcon?.ReplaceFrequentTokens());

            HybridMapper.AfterMap<ServiceInfo, ServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));

            HybridMapper.AfterMap<ServiceView, ServiceViewModel>(
                (src, dest) => dest.ServiceTypeIcon = dest.ServiceTypeIcon?.ReplaceFrequentTokens());

            HybridMapper.AfterMap<ServiceView, ServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<Dictionary<string, object>>(src.Settings, true));
           
            HybridMapper.AfterMap<ServiceViewModel, ServiceInfo>(
                (src, dest) => dest.Settings = JsonConvert.SerializeObject(src.Settings));

            #endregion
        }
    }
}
