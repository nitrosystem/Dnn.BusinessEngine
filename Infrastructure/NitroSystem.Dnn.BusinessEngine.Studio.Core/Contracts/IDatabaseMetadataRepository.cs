using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Shared;

namespace NitroSystem.Dnn.BusinessEngine.Core.Contracts
{
    public interface IDatabaseMetadataRepository
    {
        Task<IEnumerable<string>> GetDatabaseObjectsAsync(int type, string connectionString = null);

        Task<IEnumerable<TableColumnInfo>> GetDatabaseObjectColumnsAsync(string objectName, string connectionString = null);

        Task<string> GetStoredProcedureScriptAsync(string spName, string connectionString = null);

        Task<string> GetSpScript(string spName, string connectionString = null);
    }
}
