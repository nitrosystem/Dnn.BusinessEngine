using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ListItems;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Service;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IServiceFactory
    {
        Task<IEnumerable<ServiceTypeListItem>> GetServiceTypesListItemAsync();
        Task<(ServiceViewModel Service, IExtensionServiceViewModel Extension, IDictionary<string, object> ExtensionDependency)>
            GetServiceViewModelAsync(Guid scenarioId, string serviceType, Guid serviceId);
        Task<IEnumerable<ServiceViewModel>> GetServicesViewModelAsync(Guid scenarioId, string sortBy = "ViewOrder");
        Task<(IEnumerable<ServiceViewModel> Items, int? TotalCount)> GetServicesViewModelAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, string serviceType, string sortBy);
        Task<(Guid ServiceId, Guid? ExtensionServiceId)> SaveServiceAsync(ServiceViewModel service, string extensionServiceJson, bool isNew);
        Task<bool> UpdateGroupColumn(Guid serviceId, Guid? groupId);
        Task<bool> DeleteServiceAsync(Guid serviceId);
        Task<IEnumerable<ParamInfo>> GetServiceParamsAsync(Guid serviceId);
    }
}
