using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EvenFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EvenFramework.Contracts
{
    public interface IEventObserver
    {
        void OnEventDispatched(IEvent @event);
        void OnEventCompleted(IEvent @event, EventResult result);
    }
}
