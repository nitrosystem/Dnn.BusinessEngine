using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Service;
using DotNetNuke.Common.Utilities;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Service
{
    public class ServiceMappingProfile
    {
        public static void Register()
        {
            HybridMapper.BeforeMap<ServiceInfo, ServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<ServiceView, ServiceViewModel>(
                (src, dest) => dest.Settings = ReflectionUtil.TryJsonCasting<IDictionary<string, object>>(src.Settings));

            HybridMapper.BeforeMap<ServiceViewModel, ServiceInfo>(
                (src, dest) => dest.Settings = src.Settings?.ToJson());
        }
    }
}
