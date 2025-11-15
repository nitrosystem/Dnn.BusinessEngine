using System;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Contracts
{
    // observer: برای لاگرها، داشبوردها، pusherها، threshold monitorها
    public interface IEventObserver
    {
        Task OnStartAsync(EventContext context);
        Task OnNodeCompleteAsync(EventContext context, string nodeName, TimeSpan duration);
        Task OnSuccessAsync(EventContext context);
        Task OnErrorAsync(EventContext context, Exception ex);
        Task OnCancelledAsync(EventContext context);
    }
}
