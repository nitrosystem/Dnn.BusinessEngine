using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    public abstract class EngineBase<TRequest, TResponse> : IEngine<TRequest, TResponse>
    {
        public event EngineProgressHandler OnProgress;
        public event EngineErrorHandler OnError;
        public event EngineSuccessHandler<TResponse> OnSuccess;

        private readonly IServiceProvider _services;
        protected IServiceProvider Services => _services;

        private readonly EngineContext _context = new EngineContext();
        protected EngineContext Context => _context;

        protected EngineBase(IServiceProvider services)
        {
            _services = services;
        }

        public async Task<EngineResult<TResponse>> ExecuteAsync(TRequest request)
        {
            try
            {
                await NotifyProgress("Initializing", 0.0);
                await OnInitializeAsync(request);

                await NotifyProgress("Validating", 5.0);
                var validation = await ValidateAsync(request);
                if (!validation.IsSuccess)
                    return EngineResult<TResponse>.Failure(validation.Errors.ToArray());

                await NotifyProgress("BeforeExecute", 10.0);
                await BeforeExecuteAsync(request);

                await NotifyProgress("Executing", 20.0);
                var result = await ExecuteCoreAsync(request);

                await NotifyProgress("AfterExecute", 90.0);
                await AfterExecuteAsync(request, result);

                if (result.IsSuccess)
                {
                    if (OnSuccess != null) await OnSuccess(result.Data);
                    await NotifyProgress("Completed", 100.0);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (OnError != null) await OnError(ex, "Execute");
                await HandleExceptionAsync(ex);
                return EngineResult<TResponse>.Failure(ex.Message);
            }
        }

        protected virtual Task OnInitializeAsync(TRequest request) => Task.CompletedTask;
        protected virtual Task<EngineResult<object>> ValidateAsync(TRequest request) => Task.FromResult(EngineResult<object>.Success(null));
        protected virtual Task BeforeExecuteAsync(TRequest request) => Task.CompletedTask;
        protected abstract Task<EngineResult<TResponse>> ExecuteCoreAsync(TRequest request);
        protected virtual Task AfterExecuteAsync(TRequest request, EngineResult<TResponse> result) => Task.CompletedTask;
        protected virtual Task HandleExceptionAsync(Exception ex) => Task.CompletedTask;
        
        protected async Task NotifyProgress(string msg, double? percent = null)
        {
            if (OnProgress != null) await OnProgress(msg, percent);
        }
    }
}
