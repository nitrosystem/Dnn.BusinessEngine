using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.Email.Contracts
{
    public interface IEmailProvider
    {
        string Name { get; }

        Task<EmailSendResult> SendAsync(
            EmailData email,
            CancellationToken cancellationToken = default);
    }
}
