using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase.Contracts
{
    public interface IEngine<TRequest, TResponse>
    {
        Task<EngineResult<TResponse>> ExecuteAsync(TRequest request, EngineContext context);
    }
}
