using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Events;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    public abstract class EngineBase<TRequest, TResponse>
    {
        public event EngineProgressHandler OnProgress;

        protected internal EnginePipeline<TRequest, TResponse> Pipeline;

        protected EngineBase()
        {
            Pipeline = new EnginePipeline<TRequest, TResponse>();
            ConfigurePipeline(Pipeline);
        }

        protected abstract void ConfigurePipeline(EnginePipeline<TRequest, TResponse> pipeline);

        protected internal abstract TResponse CreateEmptyResponse();
      
        protected internal Task NotifyProgress(string message, double? percent = null)
            => OnProgress?.Invoke(message, percent) ?? Task.CompletedTask;

        protected internal virtual Task OnErrorAsync(Exception ex, IEngineContext context, TResponse response)
            => Task.CompletedTask;
    }

    //public abstract class EngineBase<TRequest, TResponse> : IEngineNotifier
    //{
    //    public event EngineProgressHandler OnProgress;
    //    public event EngineErrorHandler OnError;

    //    private readonly IServiceProvider _services;
    //    private readonly INotificationServer _notificationServer;
    //    private readonly EngineContext _context = new EngineContext();

    //    protected IServiceProvider Services => _services;
    //    protected EngineContext Context => _context;
    //    protected EnginePipeline<TRequest, TResponse> Pipeline { get; }

    //    protected EngineBase(IServiceProvider services, bool notify)
    //    {
    //        _services = services;
    //        Pipeline = BuildPipeline();

    //        if (notify)
    //            _notificationServer = services.GetRequiredService<INotificationServer>();
    //    }

    //    protected abstract EnginePipeline<TRequest, TResponse> BuildPipeline();

    //    public virtual TResponse CreateEmptyResponse() => Activator.CreateInstance<TResponse>();
    //    public virtual Task<TResponse> ExecutePipelineAsync(TRequest request, Func<Task<TResponse>> pipeline) => pipeline();
    //    // ⬅️ فقط عبور می‌دهد، هیچ منطقی ندارد

    //    public async Task<TResponse> ExecuteAsync(TRequest request)
    //    {
    //        await NotifyProgress("Initializing", 0);
    //        await OnInitializeAsync(request);

    //        await NotifyProgress("Validating", 5);
    //        if (!await ValidateAsync(request))
    //            return CreateEmptyResponse();

    //        await NotifyProgress("BeforeExecute", 10);
    //        await BeforeExecuteAsync(request);

    //        await NotifyProgress("Executing", 20);
    //        //var result = await Pipeline.ExecuteAsync(request, _context, _services);
    //        var result = await ExecuteCoreAsync(request);


    //        await NotifyProgress("AfterExecute", 90);
    //        await AfterExecuteAsync(request, result);

    //        await NotifyProgress("Completed", 100);
    //        return result;
    //    }

    //    protected virtual Task OnInitializeAsync(TRequest request) => Task.CompletedTask;
    //    protected virtual Task<bool> ValidateAsync(TRequest request) => Task.FromResult(true);
    //    protected virtual Task BeforeExecuteAsync(TRequest request) => Task.CompletedTask;
    //    protected abstract Task<TResponse> ExecuteCoreAsync(TRequest request);
    //    protected virtual Task AfterExecuteAsync(TRequest request, TResponse response) => Task.CompletedTask;

    //    public Task NotifyProgress(string message, double? percent)
    //        => OnProgress?.Invoke(message, percent) ?? Task.CompletedTask;

    //    public void PushingNotification(string channel, object data)
    //    {
    //        _notificationServer?.SendToChannel(channel, data);
    //    }
    //}


    //public abstract class EngineBaseTemp<TRequest, TResponse> : IEngineNotifier
    //{
    //    public event EngineProgressHandler OnProgress; 
    //    public event EngineErrorHandler OnError;

    //    private readonly IServiceProvider _services;
    //    private readonly INotificationServer _notificationServer;
    //    private readonly EngineContext _context = new EngineContext();

    //    protected IServiceProvider Services => _services;
    //    protected EngineContext Context => _context;
    //    protected EnginePipeline<TRequest, TResponse> _pipeline;

    //    protected EngineBaseTemp(IServiceProvider services, EnginePipeline<TRequest, TResponse> pipeline, bool notify)
    //    {
    //        _pipeline = pipeline;
    //        _services = services;

    //        if (notify) _notificationServer = services.GetRequiredService<INotificationServer>();
    //    }

    //    public virtual TResponse CreateEmptyResponse() => Activator.CreateInstance<TResponse>();
    //    public virtual Task<TResponse> ExecutePipelineAsync(TRequest request, Func<Task<TResponse>> pipeline) => pipeline();

    //    public async Task<TResponse> ExecuteAsync(TRequest request)
    //    {
    //        await NotifyProgress("Initializing", 0);
    //        await OnInitializeAsync(request);

    //        await NotifyProgress("Validating", 5);
    //        if (!await ValidateAsync(request))
    //            return CreateEmptyResponse(); await NotifyProgress("BeforeExecute", 10);

    //        await BeforeExecuteAsync(request);

    //        await NotifyProgress("Executing", 20);
    //        var result = await _pipeline.ExecuteAsync(request, _context, _services);

    //        await NotifyProgress("AfterExecute", 90);
    //        await AfterExecuteAsync(request, result);

    //        await NotifyProgress("Completed", 100); return result;
    //    }

    //    protected virtual Task OnInitializeAsync(TRequest request) => Task.CompletedTask;
    //    protected virtual Task<bool> ValidateAsync(TRequest request) => Task.FromResult(true);
    //    protected virtual Task BeforeExecuteAsync(TRequest request) => Task.CompletedTask;
    //    protected virtual Task AfterExecuteAsync(TRequest request, TResponse response) => Task.CompletedTask;
    //    public Task NotifyProgress(string message, double? percent) => OnProgress?.Invoke(message, percent) ?? Task.CompletedTask;
    //    public void PushingNotification(string channel, object data)
    //    {
    //        _notificationServer?.SendToChannel(channel, data);
    //    }
    //}
}
