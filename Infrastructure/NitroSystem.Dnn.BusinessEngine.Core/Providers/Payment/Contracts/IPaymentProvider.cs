using NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
