using System;
using System.Linq;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    public sealed class EngineRunner : IEngineRunner
    {
        private readonly IServiceProvider _services;

        public EngineRunner(IServiceProvider services)
        {
            _services = services;
        }

        public async Task<TResponse> RunAsync<TRequest, TResponse>(
            EngineBase<TRequest, TResponse> engine,
            TRequest request)
        {
            var context = new EngineContext();
            var response = engine.CreateEmptyResponse();

            try
            {
                Func<Task<TResponse>> next = () =>
                    Task.FromResult(response);

                foreach (var factory in engine.Pipeline.Middlewares.Reverse())
                {
                    var middleware = factory(_services);
                    var previous = next;

                    next = async () =>
                    {
                        context.CurrentPhase = EngineExecutionPhase.Middleware;
                        context.CurrentMiddleware = middleware.GetType().Name;
                        return await middleware.InvokeAsync(context, request, previous);
                    };
                }

                response = await next();
                return response;
            }
            catch (Exception ex)
            {
                await engine.OnErrorAsync(ex, context, response);
                return response;
            }
        }
    }

    //public sealed class EngineRunner : IEngineRunner
    //{
    //    private readonly IServiceProvider _services;

    //    public EngineRunner(IServiceProvider services)
    //    {
    //        _services = services;
    //    }

    //    public async Task<TResponse> RunAsync<TRequest, TResponse>(
    //        EngineBase<TRequest, TResponse> engine,
    //        TRequest request) where TResponse : class, new()
    //    {
    //        var context = new EngineContext();
    //        var response = engine.CreateEmptyResponse();

    //        try
    //        {
    //            context.CurrentPhase = EngineExecutionPhase.Initialize;
    //            await engine.OnInitializeAsync(request, context);

    //            context.CurrentPhase = EngineExecutionPhase.ValidateRequest;
    //            await engine.ValidateRequestAsync(request, context);

    //            Func<Task<TResponse>> next = () =>
    //                Task.FromResult(response);

    //            foreach (var factory in engine.Pipeline.Middlewares.Reverse())
    //            {
    //                var middleware = factory(_services);
    //                var previous = next;

    //                next = async () =>
    //                {
    //                    context.CurrentPhase = EngineExecutionPhase.Middleware;
    //                    context.CurrentMiddleware = middleware.GetType().FullName;

    //                    return await middleware.InvokeAsync(context, request, previous);
    //                };
    //            }

    //            response = await next();

    //            context.CurrentPhase = EngineExecutionPhase.ValidateResponse;
    //            await engine.ValidateResponseAsync(response, context);

    //            context.CurrentPhase = EngineExecutionPhase.Completed;
    //            await engine.OnCompletedAsync(request, response, context);

    //            return response;
    //        }
    //        catch (Exception ex)
    //        {
    //            //context.IsFaulted = true;
    //            await engine.OnErrorAsync(ex, context, response);
    //            return response;
    //        }
    //    }
    //}
}
