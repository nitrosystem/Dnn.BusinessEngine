using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Service;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IServiceFactory
    {
        #region Service Type

        Task<IEnumerable<ServiceTypeListItem>> GetServiceTypesListItemAsync(params string[] sortBy);

        #endregion

        #region Service 

        Task<IEnumerable<ServiceViewModel>> GetServicesViewModelAsync(Guid scenarioId);
        Task<(IEnumerable<ServiceViewModel> Items, int? TotalCount)> GetServicesViewModelAsync(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            string serviceDomain,
            string serviceType,
            string sortBy);
        Task<IEnumerable<ParamInfo>> GetServiceParamsAsync(Guid serviceId);
        Task<ServiceViewModel> GetServiceViewModelAsync(Guid serviceId);
        Task<(ServiceViewModel Service, IExtensionServiceViewModel Extension, IDictionary<string, object> ExtensionDependency)>
            GetServiceViewModelAsync(Guid scenarioId, string serviceType, Guid? serviceId);
        Task<string> GetServiceTypeNameAsync(Guid serviceId);

        Task<(Guid ServiceId, Guid? ExtensionServiceId)> CreateServiceAsync(ServiceViewModel service, string extensionServiceJson);
        Task UpdateGroupColumnAsync(Guid? groupId, Guid serviceId);

        Task<bool> DeleteServiceAsync(Guid serviceId);

        #endregion
    }
}
