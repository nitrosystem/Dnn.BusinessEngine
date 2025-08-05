using DotNetNuke.Collections;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping
{
    public static class ServiceMapping
    {
        public static IEnumerable<ServiceViewModel> MapServicesViewModel(IEnumerable<ServiceView> services, IEnumerable<ServiceParamInfo> serviceParams)
        {
            var itemsDict = serviceParams.GroupBy(c => c.ServiceId)
                         .ToDictionary(g => g.Key, g => g.AsEnumerable());

            return services.Select(service =>
            {
                var itemss = itemsDict.TryGetValue(service.Id, out var cols) ? cols : Enumerable.Empty<ServiceParamInfo>();
                return MapServiceViewModel(service, itemss);
            });
        }

        public static ServiceViewModel MapServiceViewModel(ServiceView service, IEnumerable<ServiceParamInfo> serviceParams)
        {
            var mapper = new ExpressionMapper<ServiceView, ServiceViewModel>();
            mapper.AddCustomMapping(src => src.ServiceTypeIcon, dest => dest.ServiceTypeIcon, src => src.ServiceTypeIcon.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions"));
            mapper.AddCustomMapping(src => src.Settings, dest => dest.Settings,
                    src => TypeCasting.TryJsonCasting<IDictionary<string, object>>(src.Settings),
                    condition => !string.IsNullOrEmpty(condition.Settings));
            mapper.AddCustomMapping(src => src, dest => dest.Params, map => serviceParams);

            var result = mapper.Map(service);
            return result;
        }
    }
}