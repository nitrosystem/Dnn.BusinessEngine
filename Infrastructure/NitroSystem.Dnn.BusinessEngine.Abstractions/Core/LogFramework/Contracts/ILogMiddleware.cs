using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Contracts
{
    public interface ILogMiddleware<TRequest>
    {
        Task<LogResult> InvokeAsync(LogContext context, TRequest request, Func<Task<LogResult>> next);
    }
}
