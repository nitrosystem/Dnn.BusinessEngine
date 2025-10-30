using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repository
{
    public class ExecuteSqlCommand : IExecuteSqlCommand
    {
        public async Task ExecuteSqlCommandTextAsync(IUnitOfWork unitOfWork, string commandText, object param = null)
        {
            try
            {
                var batches = Regex.Split(commandText, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                foreach (var batch in batches)
                {
                    var trimmed = batch.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        await unitOfWork.Connection.ExecuteAsync(trimmed, param, unitOfWork.Transaction);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error execute query {ex.Message}", ex);
            }
        }

        public static SqlDataReader ExecuteSqlReader(CommandType commandType, string commandText, Dictionary<string, object> param = null)
        {
            try
            {
                var connection = new SqlConnection(DataProvider.Instance().ConnectionString);
                var command = new SqlCommand(commandText, connection)
                {
                    CommandType = commandType
                };

                if (param != null)
                {
                    foreach (var kvp in param)
                    {
                        command.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
                    }
                }

                connection.Open();
                return command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing query: {ex.Message}", ex);
            }
        }
    }
}
