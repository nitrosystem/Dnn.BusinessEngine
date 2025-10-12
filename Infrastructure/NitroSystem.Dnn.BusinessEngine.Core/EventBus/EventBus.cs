using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EventBus
{
    public class EventBus
    {
        private readonly List<EventSubscriber> _subscribers = new List<EventSubscriber>();

        public void Subscribe(EventSubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void Publish(IEvent @event)
        {
            //foreach (var sub in _subscribers)
            //{
            //    try
            //    {
            //        if (sub.Condition(@event))
            //        {
            //            // اجرای Callback در BackgroundWorker تا async باشد
            //            var worker = new BackgroundWorker();
            //            worker.DoWork += (s, e) => sub.Callback(@event);
            //            worker.RunWorkerAsync();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Error in subscriber {sub.Id}: {ex.Message}");
            //    }
            //}
        }
    }
}
