using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.Database
{
    public class CustomQueryAction : IActionExecutor
    {
        private readonly CustomQueryService _service;

        public CustomQueryAction(CustomQueryService service)
        {
            _service = service;
        }

        public async Task<object> ExecuteAsync(ActionDto action, ConcurrentDictionary<string, object> moduleData)
        {
            if (!action.ServiceId.HasValue)
                throw new InvalidOperationException(
                    "Cannot execute action: Service Id is not specified.");

            var filledParams = HybridMapper.MapCollection<ActionParamDto, ParamInfo>(action.Params);
            return await _service.ExecuteCustomQuery(action.ServiceId.Value, filledParams);
        }
    }
}
