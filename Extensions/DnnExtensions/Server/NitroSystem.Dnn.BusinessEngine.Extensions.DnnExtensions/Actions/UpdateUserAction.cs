using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Services;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Actions
{
    public class UpdateUserAction : IActionExecutor
    {
        private readonly UpdateUserService _service;
        public UpdateUserAction(UpdateUserService service)
        {
            _service = service;
        }

        public async Task<object> ExecuteAsync(ActionDto action, ConcurrentDictionary<string, object> moduleData)
        {
			if (!action.ServiceId.HasValue)
				throw new InvalidOperationException(
					"Cannot execute action: Service Id is not specified.");

			var filledParams = HybridMapper.MapCollection<ActionParamDto, ParamInfo>(action.Params);
			await _service.UpdateUser(action.ServiceId.Value, filledParams);

            return true;
		}
    }
}
