using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Service
{
    public class ServiceFactory : IServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISql _sql;
        private readonly IServiceTypeRepository _serviceTypeRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IServiceParamRepository _serviceParamRepository;
        private readonly IServiceLocator _serviceLocator;

        public ServiceFactory(IUnitOfWork unitOfWork,
            ISql sql,
            IServiceLocator serviceLocator,
            IServiceTypeRepository serviceTypeRepository,
            IServiceRepository serviceRepository,
            IServiceParamRepository serviceParamRepository)
        {
            _unitOfWork = unitOfWork;
            _sql = sql;
            _serviceLocator = serviceLocator;
            _serviceTypeRepository = serviceTypeRepository;
            _serviceRepository = serviceRepository;
            _serviceParamRepository = serviceParamRepository;
        }

        #region Service Type

        public async Task<IEnumerable<ServiceTypeListItem>> GetServiceTypesListItemAsync(params string[] sortBy)
        {
            var serviceTypes = await _serviceTypeRepository.GetsAsync(sortBy);
            return HybridMapper.MapCollection<ServiceTypeInfo, ServiceTypeListItem>(serviceTypes);
        }

        #endregion

        #region Service

        public async Task<IEnumerable<ServiceViewModel>> GetServicesViewModelAsync(Guid scenarioId)
        {
            var services = await _serviceRepository.GetsViewAsync(scenarioId);
            return HybridMapper.MapCollection<ServiceView, ServiceViewModel>(services);
        }

        public async Task<(IEnumerable<ServiceViewModel> Items, int? TotalCount)> GetServicesViewModelAsync(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            string serviceDomain,
            string serviceType,
            string sortBy)
        {
            var results = await _serviceRepository.GetsViewAsync(scenarioId, pageIndex, pageSize, searchText, serviceDomain, serviceType, sortBy);
            var result = HybridMapper.MapWithChildren<ServiceView, ServiceViewModel, ServiceParamInfo, ServiceParamViewModel>(
                parents: results.Services,
                children: results.Params,
                parentKeySelector: p => p.Id,
                childKeySelector: c => c.ServiceId,
                assignChildren: (parent, childs) => parent.Params = childs
            );

            return (result, results.TotalCount);
        }

        public async Task<ServiceViewModel> GetServiceViewModelAsync(Guid serviceId)
        {
            var service = await _serviceRepository.GetViewAsync(serviceId);
            return HybridMapper.Map<ServiceView, ServiceViewModel>(service);
        }

        public async Task<(
            ServiceViewModel Service,
            IExtensionServiceViewModel Extension,
            IDictionary<string, object> ExtensionDependency)>
            GetServiceViewModelAsync(Guid scenarioId, string serviceType, Guid? serviceId)
        {
            (ServiceViewModel Service, IExtensionServiceViewModel Extension, IDictionary<string, object> ExtensionDependency) result = default;

            if (serviceId.HasValue)
            {
                var service = await _serviceRepository.GetViewAsync(serviceId.Value);
                var serviceParams = await _serviceParamRepository.GetsAsync(serviceId.Value, "ViewOrder");

                result.Service = HybridMapper.MapWithChildren<ServiceView, ServiceViewModel, ServiceParamInfo, ServiceParamViewModel>(
                   source: service,
                   children: serviceParams,
                   assignChildren: (parent, childs) => parent.Params = childs
               );

                if (string.IsNullOrEmpty(serviceType)) serviceType = service?.ServiceType;
            }

            var type = await _serviceRepository.GetBusinessControllerClassAsync(serviceType);
            if (!string.IsNullOrEmpty(type))
            {
                var extensionController = _serviceLocator.GetInstance<IExtensionServiceFactory>(type);
                if (serviceId.HasValue) result.Extension = await extensionController.GetService(serviceId.Value);
                result.ExtensionDependency = await extensionController.GetDependencyList(scenarioId);
            }

            return result;
        }

        public async Task<(Guid ServiceId, Guid? ExtensionServiceId)> CreateServiceAsync(ServiceViewModel service, string extensionServiceJson)
        {
            Guid? extensionServiceId = null;

            var objServiceInfo = HybridMapper.Map<ServiceViewModel, ServiceInfo>(service);
            var serviceParams = HybridMapper.MapCollection<ServiceParamViewModel, ServiceParamInfo>(service.Params);

            _unitOfWork.BeginTransaction();

            try
            {
                if (objServiceInfo.Id == Guid.Empty)
                {
                    objServiceInfo.Id = service.Id = await _serviceRepository.AddAsync(objServiceInfo);
                }
                else
                {
                    await _serviceRepository.UpdateAsync(objServiceInfo);

                    //Delete old ServiceParams
                    await _serviceParamRepository.DeletesAsync(objServiceInfo.Id);
                }

                //Add new ActionParams
                await _serviceParamRepository.BulkInsertAsync(serviceParams.Select(p => { p.ServiceId = objServiceInfo.Id; return p; }));

                //Handle Extension
                var type = await _serviceRepository.GetBusinessControllerClassAsync(service.ServiceType);
                if (!string.IsNullOrEmpty(type))
                {
                    var extensionController = _serviceLocator.CreateInstance<IExtensionServiceFactory>(type, _unitOfWork, _sql);
                    extensionServiceId = await extensionController.SaveService(service, extensionServiceJson);
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }

            return (objServiceInfo.Id, extensionServiceId);
        }

        public async Task<string> GetServiceTypeNameAsync(Guid serviceId)
        {
            return await _serviceRepository.GetServiceTypeAsync(serviceId);
        }

        public async Task UpdateGroupColumnAsync(Guid? groupId, Guid serviceId)
        {
            await _serviceRepository.UpdateGroupAsync(groupId, serviceId);
        }

        public async Task<bool> DeleteServiceAsync(Guid serviceId)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                var result = await _serviceRepository.DeleteAsync(serviceId);
                _unitOfWork.Commit();
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }

        public async Task<IEnumerable<ParamInfo>> GetServiceParamsAsync(Guid serviceId)
        {
            var serviceParams = await _serviceParamRepository.GetsAsync(serviceId);
            return HybridMapper.MapCollection<ServiceParamInfo, ParamInfo>(serviceParams);
        }

        #endregion
    }
}
