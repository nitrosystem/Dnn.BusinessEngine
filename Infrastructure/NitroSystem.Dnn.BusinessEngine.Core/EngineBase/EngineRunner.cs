using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

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
                        context.CurrentMiddleware = middleware.GetType().Name;

                        var result = await middleware.InvokeAsync(
                            context,
                            request,
                            previous,
                            (message, percent) =>
                                engine.NotifyProgress(message, percent));

                        return result;
                    };
                }

                response = await next();
                return response;
            }
            catch (Exception ex)
            {
                await engine.OnErrorAsync(context, request, response, ex);
                return response;
            }
        }
    }
}
