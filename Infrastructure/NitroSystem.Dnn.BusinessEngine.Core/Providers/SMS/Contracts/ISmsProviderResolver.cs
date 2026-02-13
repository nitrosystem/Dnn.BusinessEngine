using Microsoft.SqlServer.Server;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Contracts
{
    public interface ISmsProviderResolver
    {
        ISmsProvider Resolve(string providerName);
    }
}
