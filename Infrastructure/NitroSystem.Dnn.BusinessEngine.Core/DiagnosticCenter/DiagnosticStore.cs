using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter
{
    class DiagnosticStore : IDiagnosticStore
    {
        private readonly ISql _sql;

        public DiagnosticStore(ISql sql)
        {
            _sql = sql;
        }

        public IReadOnlyList<DiagnosticEntry> Query(DiagnosticQuery query)
        {
            throw new NotImplementedException();
        }

        public async Task Save(DiagnosticEntry entry)
        {
            await Task.Yield();
            //var id = Guid.NewGuid();
            //var json = JsonConvert.SerializeObject(entry);
            //var cmd = new SqlCommand(
            //    $@"INSERT INTO dbo.BusinessEngine_DiagnosticEntries VALUES(@Id'{id}','{entry.Context.EntryId}',N'{json.Replace("'", "''")}',GETDATE())",
            //    _connection
            //);

            //await cmd.ExecuteNonQueryAsync();
        }
    }
}
