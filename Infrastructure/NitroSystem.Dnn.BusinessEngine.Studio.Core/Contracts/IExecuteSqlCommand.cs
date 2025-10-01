using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Core.Contracts
{
    public interface IExecuteSqlCommand
    {
        Task<int> ExecuteSqlCommandTextAsync(string commandText, object param = null);
    }
}
