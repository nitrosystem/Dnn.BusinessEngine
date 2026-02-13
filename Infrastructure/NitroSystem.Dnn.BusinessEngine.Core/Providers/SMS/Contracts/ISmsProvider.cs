using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Contracts
{
    public interface ISmsProvider
    {
        string Name { get; }

        Task<SmsSendResult> SendAsync(
            SmsMessage message,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default);
    }
}
