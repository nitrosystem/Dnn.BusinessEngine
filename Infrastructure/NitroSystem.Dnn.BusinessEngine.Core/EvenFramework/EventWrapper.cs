using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EvenFramework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EvenFramework.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework
{
    public class EventWrapper
    {
        private readonly IEventDispatcher _dispatcher;
        private readonly IEnumerable<IEventPolicy> _policies;

        public EventWrapper(IEventDispatcher dispatcher, IEnumerable<IEventPolicy> policies)
        {
            _dispatcher = dispatcher;
            _policies = policies;
        }

        public EventResult Execute(string name, Func<EventResult> action)
        {
            //var @event = new BasicEvent(name);
            //_dispatcher.Dispatch(@event);

            var stopwatch = Stopwatch.StartNew();
            var memoryBefore = GC.GetTotalMemory(true);

            EventResult result;
            try
            {
                result = action.Invoke();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result = new EventResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }

            stopwatch.Stop();
            var memoryAfter = GC.GetTotalMemory(false);

            result.Duration = stopwatch.Elapsed;
            result.MemoryUsed = memoryAfter - memoryBefore;

            foreach (var policy in _policies)
            {
                //if (policy.ShouldInterrupt(@event, result))
                //{
                //    // می‌تونی اینجا عملیات رو متوقف کنی یا هشدار بدی
                //    Debug.WriteLine($"[POLICY BLOCKED] {name} - Reason: {policy.Reason}");
                //    break;
                //}
            }

            //foreach (var observer in (_dispatcher as EventDispatcher)._observers)
            //{
            //    observer.OnEventCompleted(@event, result);
            //}

            return result;
        }
    }

}
