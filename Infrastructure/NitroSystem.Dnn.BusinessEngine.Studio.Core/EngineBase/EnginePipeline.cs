using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    public class EnginePipeline<TRequest, TResponse>
    {
        private readonly List<Func<IServiceProvider, IEngineMiddleware<TRequest, TResponse>>> _middlewares
            = new List<Func<IServiceProvider, IEngineMiddleware<TRequest, TResponse>>>();

        public EnginePipeline<TRequest, TResponse> Use<TMiddleware>()
            where TMiddleware : IEngineMiddleware<TRequest, TResponse>
        {
            _middlewares.Add(sp => sp.GetRequiredService<TMiddleware>());
            return this;
        }

        public async Task<EngineResult<TResponse>> ExecuteAsync(
            TRequest request,
            EngineContext context,
            IServiceProvider services)
        {
            // ساخت chain از middlewareها
            Func<Task<EngineResult<TResponse>>> next = () =>
                Task.FromResult(EngineResult<TResponse>.Success(default(TResponse)));

            foreach (var factory in _middlewares.AsEnumerable().Reverse())
            {
                var middleware = factory(services);
                var prevNext = next;
                next = () => middleware.InvokeAsync(context, request, prevNext);
            }

            return await next();
        }
    }

}
