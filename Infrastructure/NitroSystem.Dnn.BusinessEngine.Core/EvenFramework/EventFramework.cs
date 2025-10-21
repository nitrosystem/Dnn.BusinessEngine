using Newtonsoft.Json;
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
    public abstract class EventFramework
    {
        protected readonly IEventDispatcher Dispatcher;
        protected readonly IEnumerable<IEventObserver> Observers;
        protected readonly IEnumerable<IEventPolicy> Policies;
        //protected readonly IEventStore Store;

        protected EventFramework(
            IEventDispatcher dispatcher,
            IEnumerable<IEventObserver> observers,
            IEnumerable<IEventPolicy> policies
            /*IEventStore store*/)
        {
            Dispatcher = dispatcher;
            Observers = observers;
            Policies = policies;
            //Store = store;
        }

        public EventResult Execute(string name, Func<EventResult> action)
        {
            var @event = CreateEvent(name);
            Dispatcher.Dispatch(@event);

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

            foreach (var policy in Policies)
            {
                //if (policy.ShouldInterrupt(@event, result))
                //{
                //    result.Success = false;
                //    result.ErrorMessage += $" [Policy Blocked: {policy.Reason}]";
                //    break;
                //}
            }

            foreach (var observer in Observers)
            {
                observer.OnEventCompleted(@event, result);
            }

            //Store?.Save(new EventLog
            //{
            //    Id = @event.Id,
            //    Name = @event.Name,
            //    Timestamp = @event.Timestamp,
            //    Duration = result.Duration,
            //    MemoryUsed = result.MemoryUsed,
            //    Success = result.Success,
            //    ErrorMessage = result.ErrorMessage,
            //    MetadataJson = JsonConvert.SerializeObject(@event.Metadata)
            //});

            return result;
        }

        protected abstract IEvent CreateEvent(string name);
    }
}
