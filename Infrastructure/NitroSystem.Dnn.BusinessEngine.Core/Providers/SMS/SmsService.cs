using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS
{
    public sealed class SmsService : ISmsService
    {
        private readonly ISmsProviderResolver _resolver;

        public SmsService(ISmsProviderResolver resolver)
        {
            _resolver = resolver;
        }

        public Task<SmsSendResult> SendAsync(
            SmsSendOptions options,
            SmsMessage message,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            var provider = _resolver.Resolve(options.ProviderName);
            return provider.SendAsync(message, parameters, cancellationToken);
        }
    }
}
