using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Services;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Actions
{
    public class ResetPasswordAction : IActionExecutor
    {
        private readonly ResetPasswordService _service;

        public ResetPasswordAction(ResetPasswordService service)
        {
            _service = service;
        }

        public async Task<object> ExecuteAsync(ActionDto action, ConcurrentDictionary<string, object> moduleData)
        {
			if (!action.ServiceId.HasValue)
				throw new InvalidOperationException(
					"Cannot execute action: Service Id is not specified.");

			var filledParams = HybridMapper.MapCollection<ActionParamDto, ParamInfo>(action.Params);
			return await _service.ResetPassword(action.ServiceId.Value, filledParams);
		}
    }
}
