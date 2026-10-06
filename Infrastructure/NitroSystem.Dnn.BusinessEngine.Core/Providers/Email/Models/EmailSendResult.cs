namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Models
{
    public sealed class EmailSendResult
    {
        public bool IsSuccess { get; set; }
        public string MessageId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
