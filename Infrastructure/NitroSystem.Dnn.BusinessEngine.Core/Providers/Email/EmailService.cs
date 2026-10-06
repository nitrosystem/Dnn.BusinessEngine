using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Email
{
    public sealed class EmailService : IEmailService
    {
        private readonly IEmailProviderResolver _resolver;

        public EmailService(IEmailProviderResolver resolver)
        {
            _resolver = resolver;
        }

        public Task<EmailSendResult> SendAsync(
            EmailSendOptions options,
            EmailData email,
            CancellationToken cancellationToken = default)
        {
            var provider = _resolver.Resolve(options.ProviderName);
            return provider.SendAsync(email,  cancellationToken);
        }
    }
}
