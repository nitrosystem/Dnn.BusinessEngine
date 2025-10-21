using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EvenFramework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IEnumerable<IEventObserver> _observers;

        public EventDispatcher(IEnumerable<IEventObserver> observers)
        {
            _observers = observers;
        }

        public void Dispatch(IEvent @event)
        {
            foreach (var observer in _observers)
                observer.OnEventDispatched(@event);
        }
    }

}
