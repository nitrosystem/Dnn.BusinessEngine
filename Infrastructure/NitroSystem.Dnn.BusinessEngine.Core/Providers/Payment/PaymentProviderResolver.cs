using NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Payment
{
    public sealed class PaymentProviderResolver
    {
        private readonly IReadOnlyDictionary<string, IPaymentProvider> _providers;

        public PaymentProviderResolver(IEnumerable<IPaymentProvider> providers)
        {
            _providers = providers.ToDictionary(p => p.Name);
        }

        public IPaymentProvider Resolve(string name)
            => _providers[name];
    }
}
