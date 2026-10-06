namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Contracts
{
    public interface IEmailProviderResolver
    {
        IEmailProvider Resolve(string providerName);
    }
}
