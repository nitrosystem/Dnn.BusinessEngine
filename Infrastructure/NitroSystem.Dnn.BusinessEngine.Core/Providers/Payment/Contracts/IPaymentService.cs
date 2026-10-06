using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Contracts
{
    public interface IPaymentService
    {
        Task<PaymentInitializationResult> StartAsync(
            PaymentIntent intent,
            CancellationToken cancellationToken = default);

        Task HandleWebhookAsync(
            PaymentWebhookEvent webhookEvent,
            CancellationToken cancellationToken = default);
    }
}
