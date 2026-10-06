using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Extension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Extension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Provider;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Provider
{
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _providerRepository;

        public ProviderService(IProviderRepository providerRepository)
        {
            _providerRepository = providerRepository;
        }

        public async Task<IEnumerable<ProviderListItem>> GetProvidersListItemAsync(ProviderType providerType)
        {
            var providers = await _providerRepository.GetsAsync();
            return HybridMapper.MapCollection<ProviderInfo, ProviderListItem>(providers);
        }

        public Task<ProviderViewModel> GetProviderViewModelAsync(Guid providerId)
        {
            throw new NotImplementedException();
        }

        public Task<ProviderViewModel> GetProviderViewModelByNameAsync(string providerName)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> SaveProviderAsync(ProviderViewModel provider)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteProviderAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
