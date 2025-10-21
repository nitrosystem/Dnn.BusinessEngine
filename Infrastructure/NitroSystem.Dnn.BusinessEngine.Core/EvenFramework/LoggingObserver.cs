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
    public class LoggingObserver : IEventObserver
    {
        public void OnEventDispatched(IEvent @event)
        {
            // ثبت زمان شروع، کاربر، نوع عملیات
            Debug.WriteLine($"[START] {@event.Name} at {@event.Timestamp}");
        }

        public void OnEventCompleted(IEvent @event, EventResult result)
        {
            Debug.WriteLine($"[END] {@event.Name} - Duration: {result.Duration.TotalMilliseconds}ms - Success: {result.Success}");
        }
    }

}
