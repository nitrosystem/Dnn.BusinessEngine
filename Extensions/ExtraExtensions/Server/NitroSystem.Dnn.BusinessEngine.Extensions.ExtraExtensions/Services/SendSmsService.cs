using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Services
{
    public class SendSmsService 
    {
        private readonly ISmsService _smsService;
        private readonly IServiceFactory _serviceFactory;

        public SendSmsService(ISmsService smsService, IServiceFactory serviceFactory)
        {
            _smsService = smsService;
            _serviceFactory = serviceFactory;
        }

        public async Task<SmsSendResult> SendAsync(Guid serviceId, string subject, string body, IReadOnlyList<ParamInfo> filledParams)
        {
            await Task.Yield();
            return null;
            //var service = await _serviceFactory.GetServiceViewModelAsync(action.ServiceId.Value);
            //var settings = service.Settings;
            //var options = new SmsSendOptions() { ProviderName = settings["ProviderName"]?.ToString() };

            //settings.TryGetValue("To", out var to);
            //settings.TryGetValue("Body", out var body);
            //settings.TryGetValue("TemplateCode", out var templateCode);
            //settings.TryGetValue("Parameters", out var parameters);

            //if (to != null && to.ToString().StartsWith("@")) to = ParamHelper.GetParamValue(to.ToString(), action.Params);
            //if (body != null && body.ToString().StartsWith("@")) body = ParamHelper.GetParamValue(body.ToString(), action.Params);
            //if (templateCode != null && templateCode.ToString().StartsWith("@")) templateCode = ParamHelper.GetParamValue(templateCode.ToString(), action.Params);

            //var message = new SmsMessage()
            //{
            //    To = to?.ToString(),
            //    Body = body?.ToString(),
            //    TemplateCode = templateCode?.ToString()
            //};

            //var dict = new Dictionary<string, object>();
            //if (parameters != null)
            //    foreach (var item in (JArray)parameters)
            //    {
            //        var paramName = item["ParamName"].Value<string>();
            //        if (!string.IsNullOrEmpty(paramName) && paramName.StartsWith("@"))
            //            paramName = ParamHelper.GetParamValue(paramName, action.Params)?.ToString();

            //        var paramValue = item["ParamValue"].Value<object>();
            //        if (paramValue != null && paramValue.ToString().StartsWith("@"))
            //            paramValue = ParamHelper.GetParamValue(paramValue.ToString(), action.Params)?.ToString();

            //        dict.Add(paramName, paramValue);
            //    }

            //return await _smsService.SendAsync(options, message, dict);
        }
    }
}
