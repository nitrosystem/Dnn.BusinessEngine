namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Enums
{
    public enum PaymentStatus
    {
        Created,
        Initialized,
        AwaitingUserAction,
        Processing,
        Succeeded,
        Failed,
        Canceled
    }
}
