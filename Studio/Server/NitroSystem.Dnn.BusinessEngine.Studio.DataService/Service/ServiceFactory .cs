﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ServiceLocator;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using System.Resources;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Service
{
    public class ServiceFactory : IServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;
        private readonly IServiceLocator _serviceLocator;

        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _serviceLocks = new ConcurrentDictionary<Guid, SemaphoreSlim>();

        public ServiceFactory(IUnitOfWork unitOfWork, IRepositoryBase repository, IServiceLocator serviceLocator)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _serviceLocator = serviceLocator;
        }

        public async Task<IEnumerable<ServiceTypeListItem>> GetServiceTypesListItemAsync()
        {
            var serviceTypes = await _repository.GetAllAsync<ServiceTypeView>("GroupViewOrder", "ViewOrder");

            return HybridMapper.MapCollection<ServiceTypeView, ServiceTypeListItem>(serviceTypes);
        }

        public async Task<(ServiceViewModel Service, IExtensionServiceViewModel Extension, IDictionary<string, object> ExtensionDependency)>
            GetServiceViewModelAsync(Guid scenarioId, string serviceType, Guid serviceId)
        {
            (ServiceViewModel Service, IExtensionServiceViewModel Extension, IDictionary<string, object> ExtensionDependency) result = default;

            var service = await _repository.GetAsync<ServiceView>(serviceId);
            var serviceParams = await _repository.GetByScopeAsync<ServiceParamInfo>(serviceId, "ViewOrder");

            result.Service = HybridMapper.MapWithChildren<ServiceView, ServiceViewModel, ServiceParamInfo, ServiceParamViewModel>(
               source: service,
               children: serviceParams,
               assignChildren: (parent, childs) => parent.Params = childs
           );

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

            return HybridMapper.MapCollection<ServiceView, ServiceViewModel>(services);
        }

        public async Task<(IEnumerable<ServiceViewModel> Items, int? TotalCount)> GetServicesViewModelAsync(
            Guid scenarioId, int pageIndex, int pageSize, string searchText, string serviceType, string sortBy)
        {
            var results = await _repository.ExecuteStoredProcedureMultipleAsync<int?, ServiceView, ServiceParamInfo>(
                "dbo.BusinessEngine_Studio_GetServicesWithParams", "",
                    new
                    {
                        ScenarioId = scenarioId,
                        SearchText = searchText,
                        ServiceType = serviceType,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        SortBy = sortBy
                    }
                );

            var totalCount = results.Item1?.First();
            var services = results.Item2;
            var serviceParams = results.Item3;

            var result = HybridMapper.MapWithChildren<ServiceView, ServiceViewModel, ServiceParamInfo, ServiceParamViewModel>(
                parents: services,
                children: serviceParams,
                parentKeySelector: p => p.Id,
                childKeySelector: c => c.ServiceId,
                assignChildren: (parent, childs) => parent.Params = childs
            );

            return (result, totalCount);
        }

        public async Task<(Guid ServiceId, Guid? ExtensionServiceId)> SaveServiceAsync(ServiceViewModel service, string extensionServiceJson, bool isNew)
        {
            var lockKey = service.Id == Guid.Empty ? Guid.NewGuid() : service.Id;
            var semaphore = _serviceLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            Guid? extensionServiceId = null;

            var objServiceInfo = HybridMapper.Map<ServiceViewModel, ServiceInfo>(service);
            var serviceParams = HybridMapper.MapCollection<ServiceParamViewModel, ServiceParamInfo>(service.Params);

            await semaphore.WaitAsync();

            _unitOfWork.BeginTransaction();

            try
            {
                try
                {
                    if (isNew)
                        objServiceInfo.Id = service.Id = await _repository.AddAsync<ServiceInfo>(objServiceInfo);
                    else
                    {
                        var isUpdated = await _repository.UpdateAsync<ServiceInfo>(objServiceInfo);
                        if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objServiceInfo);

                        await _repository.DeleteByScopeAsync<ServiceParamInfo>(objServiceInfo.Id);
                    }

                    foreach (var objServiceParamInfo in serviceParams ?? Enumerable.Empty<ServiceParamInfo>())
                    {
                        objServiceParamInfo.ServiceId = objServiceInfo.Id;

                        await _repository.AddAsync<ServiceParamInfo>(objServiceParamInfo);
                    }

                    var type = await _repository.GetColumnValueAsync<ServiceTypeInfo, string>("BusinessControllerClass", "ServiceType", service.ServiceType);
                    if (!string.IsNullOrEmpty(type))
                    {
                        var extensionController = _serviceLocator.CreateInstance<IExtensionServiceFactory>(type, _unitOfWork, _repository);
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

        public async Task<bool> DeleteServiceAsync(Guid serviceId)
        {
            return await _repository.DeleteAsync<ServiceInfo>(serviceId);
        }

        public async Task<IEnumerable<ParamInfo>> GetServiceParamsAsync(Guid serviceId)
        {
            var serviceParams = await _repository.GetByScopeAsync<ServiceParamInfo>(serviceId);

            return HybridMapper.MapCollection<ServiceParamInfo, ParamInfo>(serviceParams);
        }
    }
}
