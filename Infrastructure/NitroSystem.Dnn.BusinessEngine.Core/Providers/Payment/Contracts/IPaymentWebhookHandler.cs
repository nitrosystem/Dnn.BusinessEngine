using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Contracts
{
    public interface IPaymentWebhookHandler
    {
        Task HandleAsync(
            PaymentWebhookEvent webhookEvent,
            CancellationToken cancellationToken = default);
    }
}
