using System;
using System.Linq;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Email
{
    public sealed class EmailProviderResolver : IEmailProviderResolver
    {
        private readonly IReadOnlyDictionary<string, IEmailProvider> _providers;

        public EmailProviderResolver(IEnumerable<IEmailProvider> providers)
        {
            _providers = providers.ToDictionary(
                p => p.Name,
                StringComparer.OrdinalIgnoreCase);
        }

        public IEmailProvider Resolve(string providerName)
        {
            if (!_providers.TryGetValue(providerName, out var provider))
                throw new InvalidOperationException(
                    $"Email provider '{providerName}' not found.");

            return provider;
        }
    }
}
