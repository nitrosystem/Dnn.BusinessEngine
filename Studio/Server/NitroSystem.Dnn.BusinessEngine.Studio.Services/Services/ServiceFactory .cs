using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Threading;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ListItems;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class ServiceFactory : IServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;
        private readonly IServiceLocator _serviceLocator;

        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _serviceLocks = new ConcurrentDictionary<Guid, SemaphoreSlim>();

        public ServiceFactory(IUnitOfWork unitOfWork, ICacheService cacheService, IRepositoryBase repository, IServiceLocator serviceLocator)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = repository;
            _serviceLocator = serviceLocator;
        }

        #region Service Type

        public async Task<IEnumerable<ServiceTypeViewModel>> GetServiceTypeViewModelsAsync()
        {
            var serviceTypes = await _repository.GetAllAsync<ServiceTypeView>();
            return BaseMapping<ServiceTypeView, ServiceTypeViewModel>.MapViewModels(serviceTypes);
        }

        public async Task<IEnumerable<ServiceTypeListItem>> GetServiceTypesListItemAsync()
        {
            var serviceTypes = await _repository.GetAllAsync<ServiceTypeView>("GroupViewOrder", "ViewOrder");
            return serviceTypes.Select(serviceType =>
            {
                return HybridMapper.MapWithConfig<ServiceTypeView, ServiceTypeListItem>(
                   serviceType, (src, dest) =>
                   {
                       dest.Icon = (serviceType.Icon ?? string.Empty).ReplaceFrequentTokens();
                   });
            });
        }

        #endregion

        #region Service 

        public async Task<(ServiceViewModel Service, IExtensionServiceViewModel Extension, IDictionary<string, object> ExtensionDependency)>
            GetServiceViewModelAsync(Guid scenarioId, string serviceType, Guid serviceId)
        {
            (ServiceViewModel Service, IExtensionServiceViewModel Extension, IDictionary<string, object> ExtensionDependency) result = default;

            var service = await _repository.GetAsync<ServiceView>(serviceId);
            var serviceParams = await _repository.GetByScopeAsync<ServiceParamInfo>(serviceId, "ViewOrder");

            result.Service = ServiceMapping.MapServiceViewModel(service, serviceParams);

            if (serviceId != Guid.Empty)
                serviceType = service.ServiceType;

            var type = await _repository.GetColumnValueAsync<ServiceTypeInfo, string>("BusinessControllerClass", "ServiceType", serviceType);
            if (!string.IsNullOrEmpty(type))
            {
                var extensionController = _serviceLocator.GetInstance<IExtensionServiceFactory>(type);
                result.Extension = await extensionController.GetService(serviceId);
                result.ExtensionDependency = await extensionController.GetDependencyList(scenarioId);
            }

            return result;
        }

        public async Task<IEnumerable<ServiceViewModel>> GetServicesViewModelAsync(Guid scenarioId, string sortBy = "ViewOrder")
        {
            var services = await _repository.GetByScopeAsync<ServiceView>(scenarioId, sortBy);

            return ServiceMapping.MapServicesViewModel(services, Enumerable.Empty<ServiceParamInfo>());
        }

        public async Task<(IEnumerable<ServiceViewModel> Items, int? TotalCount)> GetServicesViewModelAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, string serviceType, string sortBy)
        {
            var results = await _repository.ExecuteStoredProcedureMultiGridResultAsync(
                "BusinessEngine_GetServicesWithParams", "Studio_Services_",
                    new
                    {
                        ScenarioId = scenarioId,
                        SearchText = searchText,
                        ServiceType = serviceType,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        SortBy = sortBy
                    },
                    grid => grid.ReadSingle<int?>(),
                    grid => grid.Read<ServiceView>(),
                    grid => grid.Read<ServiceParamInfo>()
                );

            var totalCount = results[0] as int?;
            var services = results[1] as IEnumerable<ServiceView>;
            var serviceParams = results[2] as IEnumerable<ServiceParamInfo>;

            return (ServiceMapping.MapServicesViewModel(services, serviceParams), totalCount);
        }

        public async Task<(Guid ServiceId, Guid? ExtensionServiceId)> SaveServiceAsync(ServiceViewModel service, string extensionServiceJson, bool isNew)
        {
            var lockKey = service.Id == Guid.Empty ? Guid.NewGuid() : service.Id;
            var semaphore = _serviceLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            Guid? extensionServiceId = null;

            var objServiceInfo = HybridMapper.MapWithConfig<ServiceViewModel, ServiceInfo>(
            service, (src, dest) =>
            {
                dest.Settings = JsonConvert.SerializeObject(service.Settings);
            });

            await semaphore.WaitAsync();

            _unitOfWork.BeginTransaction();

            try
            {
                try
                {
                    if (isNew)
                        objServiceInfo.Id =  await _repository.AddAsync<ServiceInfo>(objServiceInfo);
                    else
                    {
                        var isUpdated = await _repository.UpdateAsync<ServiceInfo>(objServiceInfo);
                        if (!isUpdated) ErrorService.ThrowUpdateFailedException(objServiceInfo);

                        await _repository.DeleteByScopeAsync<ServiceParamInfo>(objServiceInfo.Id);
                    }

                    foreach (var objServiceParamInfo in service.Params ?? Enumerable.Empty<ServiceParamInfo>())
                    {
                        objServiceParamInfo.ServiceId = objServiceInfo.Id;

                        await _repository.AddAsync<ServiceParamInfo>(objServiceParamInfo);
                    }

                    var type = await _repository.GetColumnValueAsync<ServiceTypeInfo, string>("BusinessControllerClass", "ServiceType", service.ServiceType);
                    if (!string.IsNullOrEmpty(type))
                    {
                        var extensionController = _serviceLocator.CreateInstance<IExtensionServiceFactory>(type, _unitOfWork, _cacheService, _repository);
                        extensionServiceId = await extensionController.SaveService(service, extensionServiceJson);
                    }

                    _unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    _unitOfWork.Rollback();

                    throw ex;
                }
            }
            finally
            {
                semaphore.Release();
                _serviceLocks.TryRemove(lockKey, out _);
            }

            return (objServiceInfo.Id, extensionServiceId);
        }

        public async Task<bool> UpdateGroupColumn(Guid serviceId, Guid? groupId)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                await _repository.UpdateColumnAsync<ServiceInfo>("GroupId", groupId, serviceId);

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }

            return true;
        }

        public async Task<bool> DeleteServiceAsync(Guid id)
        {
            return await _repository.DeleteAsync<ServiceInfo>(id);
        }

        #endregion

        #region Service Params

        public async Task<IEnumerable<ServiceParamViewModel>> GetServiceParamsAsync(Guid serviceId)
        {
            var serviceParams = await _repository.GetByScopeAsync<ServiceParamInfo>(serviceId);

            return BaseMapping<ServiceParamInfo, ServiceParamViewModel>.MapViewModels(serviceParams);
        }

        #endregion
    }
}
