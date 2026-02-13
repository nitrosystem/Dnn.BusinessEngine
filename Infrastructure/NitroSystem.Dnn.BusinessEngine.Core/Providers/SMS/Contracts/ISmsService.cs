using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Contracts
{
    public interface ISmsService
    {
        Task<SmsSendResult> SendAsync(
            SmsSendOptions options,
            SmsMessage message,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default);
    }
}
