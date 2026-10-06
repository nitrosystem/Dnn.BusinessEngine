using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Models
{
    public sealed class PaymentWebhookEvent
    {
        public string ProviderName { get; set; }
        public string EventType { get; set; }
        public string ProviderReference { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public IDictionary<string, object> RawData { get; set; }
    }
}
