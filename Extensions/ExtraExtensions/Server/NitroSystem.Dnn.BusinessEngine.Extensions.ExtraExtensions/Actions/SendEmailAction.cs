using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Services;
using NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Actions
{
    public class SendEmailAction : IActionExecutor
    {
        private SendEmailService _service;

        public SendEmailAction(SendEmailService service)
        {
            _service = service;
        }

        public async Task<object> ExecuteAsync(ActionDto action, ConcurrentDictionary<string, object> moduleData)
        {
            if (!action.ServiceId.HasValue)
                throw new InvalidOperationException(
                    "Cannot execute action: Service Id is not specified.");

            var subject = action.Settings["Subject"]?.ToString();
            var body = action.Settings["Body"]?.ToString();

            if (!string.IsNullOrEmpty(body))
            {
                var context = new TemplateContext(moduleData);
                var builder = new BuildTemplate();
                var parser = new TemplateParser();
                var parsedTemplate = parser.Parse(body);
                body = builder.Render(parsedTemplate, context);
            }

            var filledParams = HybridMapper.MapCollection<ActionParamDto, ParamInfo>(action.Params);
            return await _service.SendEmail(action.ServiceId.Value, subject, body, filledParams);
        }
    }
}
