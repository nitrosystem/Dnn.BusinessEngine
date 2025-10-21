using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EvenFramework.Contracts
{
    public interface IEventPolicy
    {
        bool ShouldInterrupt(IEvent @event/*, EventResult result*/);
        string Reason { get; }
    }
}
