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

    //public class EnginePipeline<TRequest, TResponse>
    //{
    //    private readonly EngineBase<TRequest, TResponse> _engine;
    //    private readonly List<Func<IServiceProvider, IEngineMiddleware<TRequest, TResponse>>> _middlewares
    //        = new List<Func<IServiceProvider, IEngineMiddleware<TRequest, TResponse>>>();

    //    public EnginePipeline(EngineBase<TRequest, TResponse> engine)
    //    {
    //        _engine = engine;
    //    }

    //    public EnginePipeline<TRequest, TResponse> Use<TMiddleware>()
    //        where TMiddleware : IEngineMiddleware<TRequest, TResponse>
    //    {
    //        _middlewares.Add(sp => sp.GetRequiredService<TMiddleware>());
    //        return this;
    //    }

    //    public async Task<TResponse> ExecuteAsync(TRequest request, IEngineContext context, IServiceProvider services)
    //    {
    //        Func<Task<TResponse>> next = () => Task.FromResult(_engine.CreateEmptyResponse());

    //        // Prepare ActionResult for later stages
    //        TResponse result = default(TResponse);

    //        foreach (var factory in _middlewares.AsEnumerable().Reverse())
    //        {
    //            var middleware = factory(services);
    //            var prevNext = next;
    //            next = async () =>
    //            {
    //                // Execute the middleware and get the ActionResult
    //                result = await middleware.InvokeAsync(context, request, prevNext, (IEngineNotifier)_engine);

    //                // Return the final response
    //                return result;
    //            };
    //        }

    //        // Execute the pipeline
    //        return await _engine.ExecutePipelineAsync(request, next);
    //    }
    //}
}
