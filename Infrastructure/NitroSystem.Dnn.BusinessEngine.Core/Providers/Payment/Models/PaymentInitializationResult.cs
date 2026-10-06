namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Models
{
    public sealed class PaymentInitializationResult
    {
        public bool IsSuccess { get; set; }
        public string RedirectUrl { get; set; } // optional
        public string ProviderReference { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
