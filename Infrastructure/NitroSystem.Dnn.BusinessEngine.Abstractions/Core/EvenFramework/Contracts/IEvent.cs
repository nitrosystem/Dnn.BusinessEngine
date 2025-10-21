using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EvenFramework.Contracts
{
    public interface IEvent
    {
        Guid Id { get; }
        string Name { get; }
        DateTime Timestamp { get; }
        IDictionary<string, object> Metadata { get; }
    }

}
