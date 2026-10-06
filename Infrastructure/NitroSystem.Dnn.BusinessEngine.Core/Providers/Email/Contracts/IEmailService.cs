using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Contracts
{
    public interface IEmailService
    {
        Task<EmailSendResult> SendAsync(
            EmailSendOptions options,
            EmailData email,
            CancellationToken cancellationToken = default);
    }
}
