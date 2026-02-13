using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Service;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IServiceFactory 
    {
        Task<IEnumerable<ServiceTypeListItem>> GetServiceTypesListItemAsync(params string[] sortBy);

        Task<IEnumerable<ServiceViewModel>> GetServicesViewModelAsync(Guid scenarioId);
        Task<ServiceViewModel> GetServiceViewModelAsync(Guid serviceId);
        Task<(ServiceViewModel Service, IExtensionServiceViewModel Extension, IDictionary<string, object> ExtensionDependency)> GetServiceViewModelAsync(Guid scenarioId, string serviceType, Guid serviceId);
        Task<(IEnumerable<ServiceViewModel> Items, int? TotalCount)> GetServicesViewModelAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, string serviceDomain, string serviceType, string sortBy);
        Task<IEnumerable<string>> GetServiceKeysAsync();
        Task<(Guid ServiceId, Guid? ExtensionServiceId)> SaveServiceAsync(ServiceViewModel service, string extensionServiceJson, bool isNew);
        Task<string> GetServiceTypeName(Guid serviceId);
        Task<bool> UpdateGroupColumn(Guid serviceId, Guid? groupId);
        Task<bool> DeleteServiceAsync(Guid serviceId);
        Task<IEnumerable<ParamInfo>> GetServiceParamsAsync(Guid serviceId);
    }
}
