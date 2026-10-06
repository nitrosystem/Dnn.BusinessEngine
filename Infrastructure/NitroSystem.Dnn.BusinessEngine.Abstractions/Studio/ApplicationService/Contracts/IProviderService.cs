using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Provider;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IProviderService
    {
        Task<IEnumerable<ProviderListItem>> GetProvidersListItemAsync(ProviderType providerType);
        Task<ProviderViewModel> GetProviderViewModelAsync(Guid providerId);
        Task<ProviderViewModel> GetProviderViewModelByNameAsync(string providerName);
        Task<Guid> SaveProviderAsync(ProviderViewModel provider);
        Task<bool> DeleteProviderAsync(Guid id);
    }
}
