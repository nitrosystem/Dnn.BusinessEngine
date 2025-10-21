using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.LogFramework
{
    public abstract class LogFrameworkBase<TRequest> : ILogFramework<TRequest>
    {
        public event Func<string, double?, Task> OnProgress;
        public event Func<Exception, string, Task> OnError;
        public event Func<TRequest, string, Task> OnLogged;

        private readonly IServiceProvider _services;
        protected IServiceProvider Services => _services;

        private readonly LogContext _context = new LogContext();
        protected LogContext Context => _context;

        protected LogFrameworkBase(IServiceProvider services)
        {
            _services = services;
        }

        public async Task<LogResult> LogAsync(TRequest request)
        {
            try
            {
                await NotifyProgress("Initializing Log", 0);
                await OnInitializeAsync(request);

                await NotifyProgress("Validating", 10);
                var validation = await ValidateAsync(request);
                if (!validation.IsSuccess)
                    return LogResult.Failure(validation.Message);

                await NotifyProgress("Before Log", 20);
                await BeforeLogAsync(request);

                var pipeline = BuildPipeline();
                await NotifyProgress("Processing Middlewares", 50);
                var result = await pipeline.ExecuteAsync(request, _context, _services);

                await NotifyProgress("After Log", 90);
                await AfterLogAsync(request, result);

                if (result.IsSuccess)
                {
                    if (OnLogged != null)
                        await OnLogged(request, result.Message);
                    await NotifyProgress("Completed", 100);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    await OnError(ex, "Log");
                await HandleExceptionAsync(ex);
                return LogResult.Failure(ex.Message);
            }
        }

        protected virtual Task OnInitializeAsync(TRequest request) => Task.CompletedTask;
        protected virtual Task<LogResult> ValidateAsync(TRequest request) => Task.FromResult(LogResult.Success());
        protected virtual Task BeforeLogAsync(TRequest request) => Task.CompletedTask;
        protected virtual Task AfterLogAsync(TRequest request, LogResult result) => Task.CompletedTask;
        protected virtual Task HandleExceptionAsync(Exception ex) => Task.CompletedTask;

        protected async Task NotifyProgress(string msg, double? percent = null)
        {
            if (OnProgress != null)
                await OnProgress(msg, percent);
        }

        protected abstract LogPipeline<TRequest> BuildPipeline();
    }

    public abstract class LogFrameworkBase
    {
        protected readonly LogPipeline _pipeline;
        protected readonly LogContext _context;

        protected LogFrameworkBase(LogPipeline pipeline, LogContext context)
        {
            _pipeline = pipeline;
            _context = context;
        }

        protected async Task LogAsync(LogLevel level, string message, string source, Exception ex = null)
        {
            var entry = new LogEntry
            {
                Level = level,
                Message = message,
                Source = source,
                Exception = ex,
                User = _context.User,
                CorrelationId = _context.CorrelationId
            };

            await _pipeline.ExecuteAsync(entry);
        }
    }
}
