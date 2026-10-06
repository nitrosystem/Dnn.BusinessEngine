using System;
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
        public IEngineContext Context;

        protected abstract void ConfigurePipeline(EnginePipeline<TRequest, TResponse> pipeline);

        protected internal abstract TResponse CreateEmptyResponse();

        protected internal virtual Task NotifyProgress(string message, double percent, bool isError = false)
            => OnProgress?.Invoke(message, percent, isError) ?? Task.CompletedTask;

        protected internal virtual Task OnErrorAsync(IEngineContext context, TRequest request, TResponse response, Exception ex)
            => Task.CompletedTask;
    }
}
