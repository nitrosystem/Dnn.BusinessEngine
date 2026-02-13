namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Models
{
    public sealed class SmsSendResult
    {
        public bool IsSuccess { get; set; }
        public string MessageId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
