using NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Contracts
{
    public interface IPaymentWebhookHandler
    {
        Task HandleAsync(
            PaymentWebhookEvent webhookEvent,
            CancellationToken cancellationToken = default);
    }
}
