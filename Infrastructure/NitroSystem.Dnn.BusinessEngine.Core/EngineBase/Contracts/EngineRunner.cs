using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts
{
    public interface IEngineRunner
    {
        Task<TResponse> RunAsync<TRequest, TResponse>(
            EngineBase<TRequest, TResponse> engine,
            TRequest request); 
    }
}
