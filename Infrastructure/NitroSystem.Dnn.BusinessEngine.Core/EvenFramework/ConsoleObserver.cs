using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework
{
    public class ConsoleObserver : IEventObserver
    {
        public Task OnStartAsync(EventContext context)
        {
            Console.WriteLine($"[Event Start] {context.EventName} ({context.Id})");
            return Task.CompletedTask;
        }

        public Task OnNodeCompleteAsync(EventContext context, string nodeName, TimeSpan duration)
        {
            Console.WriteLine($"[Node] {nodeName} completed in {duration.TotalMilliseconds}ms");
            return Task.CompletedTask;
        }

        public Task OnSuccessAsync(EventContext context)
        {
            Console.WriteLine($"[Event Success] {context.EventName} took {(context.EndTime - context.StartTime)?.TotalMilliseconds}ms");
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(EventContext context, Exception ex)
        {
            Console.WriteLine($"[Event Error] {ex.Message}");
            return Task.CompletedTask;
        }

        public Task OnCancelledAsync(EventContext context)
        {
            Console.WriteLine($"[Event Cancelled] {context.EventName}");
            return Task.CompletedTask;
        }
    }

    public class ThresholdMonitor : IEventObserver
    {
        private readonly TimeSpan _maxNodeDuration;
        public ThresholdMonitor(TimeSpan maxNodeDuration) => _maxNodeDuration = maxNodeDuration;

        public Task OnStartAsync(EventContext context) => Task.CompletedTask;

        public Task OnNodeCompleteAsync(EventContext context, string nodeName, TimeSpan duration)
        {
            if (duration > _maxNodeDuration)
            {
                context.AddNode($"Node {nodeName} exceeded max duration {_maxNodeDuration.TotalMilliseconds}ms");
                //context.AddNode("Warning", $"Node {nodeName} exceeded max duration {_maxNodeDuration.TotalMilliseconds}ms", "ThresholdMonitor");
                // اگر بخواهیم عملیات را قطع کنیم:
                if (context.Items.TryGetValue("__cts", out var obj) && obj is CancellationTokenSource cts)
                {
                    cts.Cancel();
                    context.AddNode("Cancelled by ThresholdMonitor");
                }
            }
            return Task.CompletedTask;
        }

        public Task OnSuccessAsync(EventContext context) => Task.CompletedTask;
        public Task OnErrorAsync(EventContext context, Exception ex) => Task.CompletedTask;
        public Task OnCancelledAsync(EventContext context) => Task.CompletedTask;
    }
}
