using DotNetNuke.Collections;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Common.TypeCasting;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping
{
    public static class ServiceMapping
    {
        //#region Service Type Mapping

        //public static IEnumerable<ServiceTypeViewModel> GetServiceTypesViewModel()
        //{
        //    var serviceTypes = ServiceTypeRepository.Instance.GetServiceTypes();

        //    return GetServiceTypesViewModel(serviceTypes);
        //}

        //public static IEnumerable<ServiceTypeViewModel> GetServiceTypesViewModel(IEnumerable<ServiceTypeView> serviceTypes)
        //{
        //    var result = new List<ServiceTypeViewModel>();

        //    foreach (var objServiceTypeView in serviceTypes ?? Enumerable.Empty<ServiceTypeView>())
        //    {
        //        var serviceType = GetServiceTypeViewModel(objServiceTypeView);
        //        result.Add(serviceType);
        //    }

        //    return result;
        //}

        //public static ServiceTypeViewModel GetServiceTypeViewModel(ServiceTypeView objServiceTypeView)
        //{

        //}

        //#endregion

        #region Service Mapping

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
            mapper.AddCustomMapping(src => src.ServiceTypeIcon, dest => dest.ServiceTypeIcon, source => source.ServiceTypeIcon.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions"));
            mapper.AddCustomMapping(src => src.AuthorizationRunService, dest => dest.AuthorizationRunService,
                source => source.AuthorizationRunService.Split(','),
                condition => !string.IsNullOrEmpty(condition.AuthorizationRunService));
            mapper.AddCustomMapping(src => src.ResultType, dest => dest.ResultType, source => (ServiceResultType)source.ResultType);
            mapper.AddCustomMapping(src => src.Settings, dest => dest.Settings,
                    source => TypeCastingUtil<IDictionary<string, object>>.TryJsonCasting(source.Settings),
                    condition => !string.IsNullOrEmpty(condition.Settings));
            mapper.AddCustomMapping(src => src, dest => dest.Params, map => serviceParams);

            var result = mapper.Map(service);
            return result;
        }

        #endregion
    }
}