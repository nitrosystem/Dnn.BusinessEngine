using System;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts
{
    public interface IEngineMiddleware<TRequest, TResponse>
    {
        Task<TResponse> InvokeAsync(IEngineContext context, TRequest request, Func<Task<TResponse>> next);
    }
}
