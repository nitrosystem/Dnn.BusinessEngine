using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts
{
    public interface IExecuteSqlCommand
    {
        Task ExecuteSqlCommandTextAsync(IUnitOfWork unitOfWork, string commandText, object param = null);
        Task<IEnumerable<T>> ExecuteSqlCommandTextAsync<T>(IUnitOfWork unitOfWork, string commandText, object param = null);
        IDataReader ExecuteSqlReader(IUnitOfWork unitOfWork, CommandType commandType, string commandText, object param = null);
    }
}
