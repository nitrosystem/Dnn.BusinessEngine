namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Contracts
{
    public interface ISmsProviderResolver
    {
        ISmsProvider Resolve(string providerName);
    }
}
