using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Contracts
{
    public interface ILogMiddleware<TContext>
    {
        Task<LogResult> InvokeAsync(TContext context, Func<Task<LogResult>> next);
    }
}
