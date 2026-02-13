using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts
{
    public interface IDatabaseMetadataRepository
    {
        Task<IEnumerable<string>> GetDatabaseObjectsAsync(int type);

        Task<List<TableColumnInfo>> GetDatabaseObjectColumnsAsync(string objectName);

        Task<string> GetStoredProcedureScriptAsync(string spName);

        Task<string> GetSpScript(string spName);

        Task<string> BuildCreateTableScript(string schema, string table);
    }
}
