using DotNetNuke.Data;
using DotNetNuke.Services.Scheduling;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Shared.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Shared.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Services
{
    public class ServiceFactory : IServiceFactory
    {
        private readonly IRepositoryBase _repository;

        public ServiceFactory(IRepositoryBase repository)
        {
            _repository = repository;
        }

        public async Task<ServiceDto> GetServiceDtoAsync(Guid serviceId)
        {
            var results = await _repository.ExecuteStoredProcedureMultiGridResultAsync(
                "BusinessEngine_App_GetService", "App_Service_",
                    new
                    {
                        ServiceId = serviceId,
                    },
                    grid => grid.ReadSingle<ServiceInfo>(),
                    grid => grid.Read<ServiceParamInfo>()
                );

            var service = results[0] as ServiceInfo;
            var serviceParams = results[1] as IEnumerable<ServiceParamInfo>;

            return HybridMapper.MapWithConfig<ServiceInfo, ServiceDto>(service,
                (src, dest) =>
                {
                    dest.Settings = TypeCasting.TryJsonCasting<IDictionary<string, object>>(service.Settings);
                });
        }
    }
}
