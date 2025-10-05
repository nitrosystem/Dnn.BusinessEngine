using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase.Events;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    // کلاس انتزاعی با Template Method
    public abstract class EngineBase<TRequest, TResponse> : IEngine<TRequest, TResponse>
    {
        // قابل subscribe توسط caller یا فریم‌ورک
        public event EngineProgressHandler OnProgress;
        public event EngineErrorHandler OnError;
        public event EngineSuccessHandler<TResponse> OnSuccess;

        protected IServiceProvider Services => _services;
        private readonly IServiceProvider _services;
       
        protected readonly EngineContext Context;

        protected EngineBase(EngineContext context)
        {
            Context = context;
        }

        public async Task<EngineResult<TResponse>> ExecuteAsync(TRequest request, EngineContext context)
        {
            try
            {
                await NotifyProgress("Initializing", 0.0);
                await OnInitializeAsync(request, context);

                await NotifyProgress("Validating", 5.0);
                var validation = await ValidateAsync(request, context);
                if (!validation.IsSuccess)
                    return EngineResult<TResponse>.Failure(validation.Errors.ToArray());

                await NotifyProgress("BeforeExecute", 10.0);
                await BeforeExecuteAsync(request, context);

                await NotifyProgress("Executing", 20.0);
                var result = await ExecuteCoreAsync(request, context);

                await NotifyProgress("AfterExecute", 90.0);
                await AfterExecuteAsync(request, context, result);

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
                await HandleExceptionAsync(ex, context);
                return EngineResult<TResponse>.Failure(ex.Message);
            }
        }

        // hooks (قابل override)
        protected virtual Task OnInitializeAsync(TRequest request, EngineContext context) => Task.CompletedTask;
        protected virtual Task<EngineResult<object>> ValidateAsync(TRequest request, EngineContext context) => Task.FromResult(EngineResult<object>.Success(null));
        protected virtual Task BeforeExecuteAsync(TRequest request, EngineContext context) => Task.CompletedTask;
        protected abstract Task<EngineResult<TResponse>> ExecuteCoreAsync(TRequest request, EngineContext context);
        protected virtual Task AfterExecuteAsync(TRequest request, EngineContext context, EngineResult<TResponse> result) => Task.CompletedTask;
        protected virtual Task HandleExceptionAsync(Exception ex, EngineContext context) => Task.CompletedTask;

        // helper برای ارسال پروگرس
        protected async Task NotifyProgress(string msg, double? percent = null)
        {
            if (OnProgress != null) await OnProgress(msg, percent);
        }
    }
}
