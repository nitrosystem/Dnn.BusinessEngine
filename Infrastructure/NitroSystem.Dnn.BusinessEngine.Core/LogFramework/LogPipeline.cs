using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.LogFramework
{
    public class LogPipeline<TContext>
    {
        private readonly List<Func<IServiceProvider, ILogMiddleware<TContext>>> _middlewares
            = new List<Func<IServiceProvider, ILogMiddleware<TContext>>>();

        public LogPipeline<TContext> Use<TMiddleware>()
            where TMiddleware : ILogMiddleware<TContext>
        {
            _middlewares.Add(sp => sp.GetRequiredService<TMiddleware>());
            return this;
        }

        public async Task<LogResult> ExecuteAsync(TContext context, IServiceProvider services, Func<Task<LogResult>> next)
        {
            foreach (var factory in _middlewares.AsEnumerable().Reverse())
            {
                var middleware = factory(services);
                var prevNext = next;
                next = () => middleware.InvokeAsync(context, prevNext);
            }

            return await next();
        }
    }
}
