using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter
{
    class DiagnosticStore : IDiagnosticStore
    {
        private readonly SqlConnection _connection;

        public DiagnosticStore()
        {
            _connection = new(DotNetNuke.Data.DataProvider.Instance().ConnectionString);
            _connection.Open();
        }

        public async Task Save(DiagnosticEntry entry)
        {
            var id = Guid.NewGuid();
            var json = JsonConvert.SerializeObject(entry);
            var cmd = new SqlCommand(
                $@"INSERT INTO dbo.BusinessEngine_DiagnosticEntries VALUES('{id}','{entry.Context.EntryId}',N'{json}',GETDATE())",
                _connection
            );

            await cmd.ExecuteNonQueryAsync();

        }
    }
}
