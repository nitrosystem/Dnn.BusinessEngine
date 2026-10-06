using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Models;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Services
{
    public class SendEmailService
    {
        private readonly IServiceFactory _serviceFactory;
        private readonly IEmailService _emailService;

        public SendEmailService(IServiceFactory serviceFactory, IEmailService emailService)
        {
            _serviceFactory = serviceFactory;
            _emailService = emailService;
        }

        // --- Constants to remove magic strings ---
        private static class EmailKeys
        {
            public const string From = "From";
            public const string To = "To";
            public const string Subject = "Subject";
            public const string Body = "Body";
        }

        public async Task<EmailSendResult> SendEmail(Guid serviceId, string subject, string body, IReadOnlyList<ParamInfo> filledParams)
        {
            var service = await _serviceFactory.GetServiceViewModelAsync(serviceId)
               ?? throw new InvalidOperationException($"Service '{serviceId}' was not found.");

            var settings = service.Settings ?? new Dictionary<string, object>();
            var paramLookup = ParamHelper.GetLookup(filledParams);
            string from = ParamHelper.ResolveValue(settings.GetValueOrDefault(EmailKeys.From), paramLookup);
            string to = ParamHelper.ResolveValue(settings.GetValueOrDefault(EmailKeys.To), paramLookup);

            var options = new EmailSendOptions() { ProviderName = settings["ProviderName"]?.ToString() };
            var email = new EmailData()
            {
                From = from,
                To = to,
                Subject = subject,
                Body = body
            };

            return await _emailService.SendAsync(options, email);
        }
    }
}
