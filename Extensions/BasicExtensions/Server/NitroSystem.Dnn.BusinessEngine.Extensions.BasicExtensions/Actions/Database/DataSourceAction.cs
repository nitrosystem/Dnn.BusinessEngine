using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.Database
{
    public class DataSourceAction : IActionExecutor
    {
        private readonly DataSourceService _service;

        public DataSourceAction(DataSourceService service)
        {
            _service = service;
        }

        public async Task<object> ExecuteAsync(ActionDto action, ConcurrentDictionary<string, object> moduleData)
        {
            if (!action.ServiceId.HasValue)
                throw new InvalidOperationException(
                    "Cannot execute action: Service Id is not specified.");

            var filledParams = HybridMapper.MapCollection<ActionParamDto, ParamInfo>(action.Params);
            var data = await _service.GetDataSourceService(action.ServiceId.Value, filledParams);
            return new ListResult()
            {
                Items = data.Items,
                TotalCount = data.TotalCount
            };
        }
    }
}
