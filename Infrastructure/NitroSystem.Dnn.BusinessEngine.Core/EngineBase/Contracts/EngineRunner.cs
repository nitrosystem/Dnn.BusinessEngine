using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts
{
    public interface IEngineRunner
    {
        Task<TResponse> RunAsync<TRequest, TResponse>(
            EngineBase<TRequest, TResponse> engine,
            TRequest request) where TResponse : class, new();
    }
}
