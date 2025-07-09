using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.ADO_NET
{
    public interface ISqlQueryExecutor
    {
        List<T> ExecuteQuery<T>(string queryOrSp, CommandType commandType, Dictionary<string, object> parameters = null) where T : new();
    }
}
