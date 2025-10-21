using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.LogFramework
{
    public class LogPipeline<TRequest>
    {
        private readonly List<Func<IServiceProvider, ILogMiddleware<TRequest>>> _middlewares =
            new List<Func<IServiceProvider, ILogMiddleware<TRequest>>>();

        public LogPipeline<TRequest> Use<TMiddleware>()
            where TMiddleware : ILogMiddleware<TRequest>
        {
            _middlewares.Add(sp => sp.GetRequiredService<TMiddleware>());
            return this;
        }

        public async Task<LogResult> ExecuteAsync(TRequest request, LogContext context, IServiceProvider services)
        {
            Func<Task<LogResult>> next = () => Task.FromResult(LogResult.Success("No middleware executed"));

            foreach (var factory in _middlewares.AsEnumerable().Reverse())
            {
                var middleware = factory(services);
                var prevNext = next;
                next = () => middleware.InvokeAsync(context, request, prevNext);
            }

            return await next();
        }
    }

    public class LogPipeline
    {
        private readonly List<Func<ILogMiddleware>> _middlewares = new();
        private readonly List<ILogHandler> _handlers = new();

        public LogPipeline Use<T>() where T : ILogMiddleware, new()
        {
            _middlewares.Add(() => new T());
            return this;
        }

        public LogPipeline Use(Func<ILogMiddleware> factory)
        {
            _middlewares.Add(factory);
            return this;
        }

        public LogPipeline AddHandler(ILogHandler handler)
        {
            _handlers.Add(handler);
            return this;
        }

        public async Task ExecuteAsync(LogEntry entry)
        {
            Func<Task> next = async () =>
            {
                foreach (var handler in _handlers)
                    await handler.WriteAsync(entry);
            };

            foreach (var middlewareFactory in _middlewares.AsEnumerable().Reverse())
            {
                var middleware = middlewareFactory();
                var currentNext = next;
                next = () => middleware.InvokeAsync(entry, currentNext);
            }

            await next();
        }
    }
}
