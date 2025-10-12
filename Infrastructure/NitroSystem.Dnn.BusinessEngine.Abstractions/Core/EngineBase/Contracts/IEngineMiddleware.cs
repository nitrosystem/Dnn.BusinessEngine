﻿using System;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts
{
    public interface IEngineMiddleware<TRequest, TResponse>
    {
        Task<EngineResult<TResponse>> InvokeAsync(IEngineContext context, TRequest request, Func<Task<EngineResult<TResponse>>> next);
    }
}
