using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Service;
using NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Components
{
    public class ServiceComponent : ComponentBase
    {
        private readonly IServiceFactory _service;
        private readonly ServiceScope _scope;
        private readonly object[] _params;

        public ServiceComponent(IServiceFactory service, ServiceScope scope, params object[] args)
        {
            _service = service;
            _scope = scope;
            _params = args;

            Name = "Service";
        }

        public override void CreateTasks()
        {
            switch (_scope)
            {
                case ServiceScope.ScenarioServices:
                    Tasks.Enqueue(new GetModelsAsJsonTask<IEnumerable<ServiceViewModel>>(_service, "GetServicesViewModelAsync", _params));
                    break;
                case ServiceScope.OnlyOneService:
                    Tasks.Enqueue(new GetModelsAsJsonTask<ServiceViewModel>(_service, "GetServiceViewModelAsync", (service) =>
                    {
                        service.ServiceName = "[[ServiceName]]";
                    }, _params));
                    break;
            }
        }

        private async Task<ServiceViewModel> GetService(Guid serviceId)
        {
            return await _service.getser(serviceId);
        }
    }
}