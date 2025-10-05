using System;
using System.Collections.Concurrent;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts
{
    public interface IExpressionService
    {
        object Evaluate(string expression, ConcurrentDictionary<string, object> data);

        Action<object> BuildDataSetter(string path, ConcurrentDictionary<string, object> data);
    }
}
