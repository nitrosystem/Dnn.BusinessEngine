using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Contracts
{
    public interface IExpressionService
    {
        object Evaluate(string expression, ConcurrentDictionary<string, object> data);

        Action<object> BuildDataSetter(string path, ConcurrentDictionary<string, object> data);
    }
}
