using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.Database
{
    public class DataRowAction : IActionExecutor
    {
        private readonly DataRowService _service;

        public DataRowAction(DataRowService service)
        {
            _service = service;
        }

        public async Task<object> ExecuteAsync(ActionDto action, ConcurrentDictionary<string, object> moduleData)
        {
            if (!action.ServiceId.HasValue)
                throw new InvalidOperationException(
                    "Cannot execute action: Service Id is not specified.");

            var filledParams = HybridMapper.MapCollection<ActionParamDto, ParamInfo>(action.Params);
            return await _service.GetDataRowService(action.ServiceId.Value, filledParams);
        }
    }
}
