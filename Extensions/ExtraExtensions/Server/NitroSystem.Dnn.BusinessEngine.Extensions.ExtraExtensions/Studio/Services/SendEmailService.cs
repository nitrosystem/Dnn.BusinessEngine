using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Enums;


namespace NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Studio.Services
{
    public class SendEmailService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IProviderService _providerService;

        public SendEmailService(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            IAppModelService appModelService,
            IProviderService providerService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _providerService = providerService;
        }

        public async Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId)
        {
            var providers = await _providerService.GetProvidersListItemAsync(ProviderType.Email);
            return new Dictionary<string, object>
            {
                { "Providers", providers }
            };
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            await Task.Yield();
            return null;
        }

        public async Task<Guid> SaveService(ServiceViewModel parentService, string extensionServiceJson)
        {
            await Task.Yield();
            return Guid.Empty;
        }
    }
}
