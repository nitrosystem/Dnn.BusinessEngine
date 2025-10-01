using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Action;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Service;
using DotNetNuke.Common.Utilities;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.MappingConfiguration
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
