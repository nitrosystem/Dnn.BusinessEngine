using System;
using System.Linq;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS
{
    public sealed class SmsProviderResolver : ISmsProviderResolver
    {
        private readonly IReadOnlyDictionary<string, ISmsProvider> _providers;

        public SmsProviderResolver(IEnumerable<ISmsProvider> providers)
        {
            _providers = providers.ToDictionary(
                p => p.Name,
                StringComparer.OrdinalIgnoreCase);
        }

        public ISmsProvider Resolve(string providerName)
        {
            if (!_providers.TryGetValue(providerName, out var provider))
                throw new InvalidOperationException(
                    $"SMS provider '{providerName}' not found.");

            return provider;
        }
    }
}
