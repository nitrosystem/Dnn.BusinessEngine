using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.StudioDataService
{
    public class WebServiceRestfulService : IExtensionServiceFactory
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;
        private readonly IAppModelService _appModelService;

        public WebServiceRestfulService(IRepositoryBase repository, IUnitOfWork unitOfWork, IAppModelService appModelService)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _appModelService = appModelService;
        }

        public async Task<IExtensionServiceViewModel> GetService(Guid serviceId)
        {
            var objWebServiceRestfulInfo = await _repository.GetByColumnAsync<WebServiceRestfulInfo>("ServiceId", serviceId);

            return objWebServiceRestfulInfo != null
                ? HybridMapper.Map<WebServiceRestfulInfo, WebServiceRestfulViewModel>(objWebServiceRestfulInfo)
                : null;
        }

        public async Task<IDictionary<string, object>> GetDependencyList(Guid scenarioId)
        {
            var appModels = await _appModelService.GetAppModelsListItemAsync(scenarioId, "ModelName");

            return new Dictionary<string, object>
            {
                { "AppModels", appModels }
            };
        }

        public async Task<Guid> SaveService(IViewModel parentService, string extensionServiceJson)
        {
            var service = parentService as ServiceViewModel;
            var restfulService = JsonConvert.DeserializeObject<WebServiceRestfulViewModel>(extensionServiceJson);

            var objWebServiceRestfulInfo = HybridMapper.Map<WebServiceRestfulViewModel, WebServiceRestfulInfo>(restfulService);
            objWebServiceRestfulInfo.ServiceId = service.Id;

            if (objWebServiceRestfulInfo.Id == Guid.Empty)
                objWebServiceRestfulInfo.Id = await _repository.AddAsync<WebServiceRestfulInfo>(objWebServiceRestfulInfo);
            else
                await _repository.UpdateAsync<WebServiceRestfulInfo>(objWebServiceRestfulInfo);

            return objWebServiceRestfulInfo.Id;
        }
    }
}
