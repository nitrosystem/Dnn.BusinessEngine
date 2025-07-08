using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NitroSystem.Dnn.BusinessEngine.Core.Cashing;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class ServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        public ServiceFactory(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = new RepositoryBase(_unitOfWork, _cacheService);
        }

        #region Service Type

        #endregion

        public async Task<IEnumerable<ServiceTypeViewModel>> GetServiceTypeViewModelsAsync()
        {
            var serviceTypes = await _repository.GetAllAsync<ServiceTypeView>();
            return BaseMapping<ServiceTypeView, ServiceTypeViewModel>.MapViewModels(serviceTypes);
        }

        #region Service 

        public async Task<ServiceViewModel> GetServiceViewModelAsync(Guid id)
        {
            var service = await _repository.GetAsync<ServiceView>(id);
            var serviceParams = await _repository.GetByScopeAsync<ServiceParamInfo>(id);
            return ServiceMapping.MapServiceViewModel(service, serviceParams);
        }

        public async Task<(IEnumerable<ServiceViewModel> Items, int TotalCount)> GetServicesViewModelAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, string serviceType, string serviceSubtype, string sortBy)
        {
            var result = await _repository.ExecuteStoredProcedureAsListWithPagingAsync<ServiceView>("BusinessEngine_GetServices",
                        new
                        {
                            ScenarioId = scenarioId,
                            SearchText = searchText,
                            ServiceType = serviceType,
                            ServiceSubtype = serviceSubtype,
                            PageIndex = pageIndex,
                            PageSize = pageSize,
                            SortBy = sortBy
                        });

            var services = result.Item1;
            var totalCount = result.Item2;

            return (ServiceMapping.MapServicesViewModel(services, Enumerable.Empty<ServiceParamInfo>()), totalCount);
        }

        public async Task<Guid> SaveServiceAsync(ServiceViewModel service, bool isNew)
        {
            var objServiceInfo = ServiceMapping.MapServiceInfo(service);

            if (isNew)
                objServiceInfo.Id = await _repository.AddAsync<ServiceInfo>(objServiceInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<ServiceInfo>(objServiceInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objServiceInfo);
            }

            await _repository.DeleteByScopeAsync<ServiceParamInfo>(objServiceInfo.Id);

            foreach (var objServiceParamInfo in service.Params)
            {
                await _repository.AddAsync<ServiceParamInfo>(objServiceParamInfo);
            }

            return objServiceInfo.Id;
        }

        public async Task<bool> DeleteServiceAsync(Guid id)
        {
            return await _repository.DeleteAsync<ServiceInfo>(id);
        }

        #endregion

        #region Service Params

        public async Task<IEnumerable<ServiceParamViewModel>> GetServiceParamsAsync(Guid serviceId)
        {
            var serviceParams = await _repository.GetByScopeAsync<ServiceView>(serviceId);

            return BaseMapping<ServiceView, ServiceParamViewModel>.MapViewModels(serviceParams);
        }

        #endregion
    }
}
