using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ListItems;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IServiceFactory
    {
        #region Service Type

        Task<IEnumerable<ServiceTypeListItem>> GetServiceTypesListItemAsync();

        Task<IEnumerable<ServiceTypeViewModel>> GetServiceTypeViewModelsAsync();

        #endregion

        #region Service 

        Task<(ServiceViewModel Service, IExtensionServiceViewModel Extension, IDictionary<string, object> ExtensionDependency)>
            GetServiceViewModelAsync(Guid scenarioId, string serviceType, Guid serviceId);

        Task<IEnumerable<ServiceViewModel>> GetServicesViewModelAsync(Guid scenarioId, string sortBy = "ViewOrder");

        Task<(IEnumerable<ServiceViewModel> Items, int? TotalCount)> GetServicesViewModelAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, string serviceType, string sortBy);

        Task<(Guid ServiceId, Guid? ExtensionServiceId)> SaveServiceAsync(ServiceViewModel service, string extensionServiceJson, bool isNew);

        Task<bool> UpdateGroupColumn(Guid serviceId, Guid? groupId);

        Task<bool> DeleteServiceAsync(Guid serviceId);

        #endregion

        #region Service Params

        Task<IEnumerable<ServiceParamViewModel>> GetServiceParamsAsync(Guid serviceId);

        #endregion
    }
}
