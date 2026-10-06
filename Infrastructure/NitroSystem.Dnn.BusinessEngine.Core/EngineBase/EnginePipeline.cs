using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    public sealed class EnginePipeline<TRequest, TResponse>
    {
        internal IList<Func<IServiceProvider, IEngineMiddleware<TRequest, TResponse>>> Middlewares
            = new List<Func<IServiceProvider, IEngineMiddleware<TRequest, TResponse>>>();

        public EnginePipeline<TRequest, TResponse> Use<TMiddleware>()
            where TMiddleware : IEngineMiddleware<TRequest, TResponse>
        {
            Middlewares.Add(sp =>
                (IEngineMiddleware<TRequest, TResponse>)sp.GetRequiredService(typeof(TMiddleware)));

            return this;
        }
    }
}
