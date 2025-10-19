using System;
using System.Threading.Tasks;
using DotNetNuke.Services.Installer.Log;

namespace NitroSystem.Dnn.BusinessEngine.Core.LogFramework
{
        public abstract class LogFrameworkBase<TContext>
        {
            public event Func<LogEntry, Task> OnLog;
            public event Func<Exception, Task> OnError;

            private readonly IServiceProvider _services;
            protected IServiceProvider Services => _services;

            private readonly LogPipeline<TContext> _pipeline = new();
            protected LogPipeline<TContext> Pipeline => _pipeline;

            protected readonly LogContext Context = new();
            protected LogFrameworkBase(IServiceProvider services)
            {
                _services = services;
            }

            public async Task<LogResult> TrackAsync(Func<Task> action, string scenarioName, object meta = null)
            {
                try
                {
                    Context.StartScenario(scenarioName, meta);

                    await _pipeline.ExecuteAsync(Context, _services, async () =>
                    {
                        await action();
                        return LogResult.Success();
                    });

                    Context.CompleteScenario();
                    return LogResult.Success();
                }
                catch (Exception ex)
                {
                    Context.FailScenario(ex);
                    if (OnError != null)
                        await OnError(ex);

                    return LogResult.Failure(ex.Message);
                }
            }

            protected async Task WriteAsync(string message, LogLevel level = LogLevel.Info, object data = null)
            {
                var entry = new LogEntry
                {
                    ScenarioId = Context.ScenarioId,
                    ScenarioName = Context.ScenarioName,
                    Timestamp = DateTime.UtcNow,
                    Level = level,
                    Message = message,
                    Data = data
                };

                if (OnLog != null)
                    await OnLog(entry);

                await Context.AddEntryAsync(entry);
            }

            protected abstract Task InitializeAsync();
        }
    }
