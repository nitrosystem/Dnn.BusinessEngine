using System.Data;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.ADO_NET
{
    public interface ISqlQueryExecutor
    {
        List<T> ExecuteQuery<T>(string queryOrSp, CommandType commandType, Dictionary<string, object> parameters = null) where T : new();
    }
}
