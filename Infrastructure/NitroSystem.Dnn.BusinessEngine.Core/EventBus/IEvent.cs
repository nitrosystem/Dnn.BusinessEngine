using System;
using System.Collections.Generic;


namespace NitroSystem.Dnn.BusinessEngine.Core.EventBus
{
    public interface IEvent
    {
        string Name { get; }
        DateTime OccurredAt { get; }
        IDictionary<string, object> Data { get; }
    }
}
