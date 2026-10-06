using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Contracts
{
    public interface IPaymentProvider
    {
        string Name { get; }

        Task<PaymentInitializationResult> InitializeAsync(
            PaymentIntent intent,
            CancellationToken cancellationToken = default);

        Task<PaymentVerificationResult> VerifyAsync(
            PaymentIntent intent,
            CancellationToken cancellationToken = default);
    }

}
