using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Events;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    public abstract class EngineBase<TRequest, TResponse>
    {
        protected readonly IDiagnosticStore DiagnosticStore;
        protected readonly Guid TraceId;

        protected internal EnginePipeline<TRequest, TResponse> Pipeline;

        protected EngineBase(IDiagnosticStore diagnosticStore)
        {
            DiagnosticStore = diagnosticStore;
            TraceId = Guid.NewGuid();

            Pipeline = new EnginePipeline<TRequest, TResponse>();
            ConfigurePipeline(Pipeline);
        }

        public event EngineProgressHandler OnProgress;

        protected abstract void ConfigurePipeline(EnginePipeline<TRequest, TResponse> pipeline);

        protected internal abstract TResponse CreateEmptyResponse();

        protected internal virtual Task NotifyProgress(string channel, string message, double percent)
            => OnProgress?.Invoke(channel, message, percent) ?? Task.CompletedTask;

        protected internal virtual Task OnErrorAsync(IEngineContext context, TRequest request, TResponse response, Exception ex)
            => Task.CompletedTask;
    }
}
