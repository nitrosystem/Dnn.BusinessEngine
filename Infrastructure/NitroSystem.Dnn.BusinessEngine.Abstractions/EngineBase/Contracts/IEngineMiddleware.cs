using System;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase.Contracts
{
    public interface IEngineMiddleware<TRequest, TResponse>
    {
        Task<EngineResult<TResponse>> InvokeAsync(
            EngineContext context,
            TRequest request,
            Func<Task<EngineResult<TResponse>>> next);
    }
}
