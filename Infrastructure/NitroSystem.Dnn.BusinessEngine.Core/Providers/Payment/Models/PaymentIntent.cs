using NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Models
{
    public sealed class PaymentIntent
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public PaymentStatus Status { get; private set; }

        public string ProviderName { get; set; }
        public string? ProviderReference { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public IDictionary<string, object> Metadata { get; set; }
    }
}
