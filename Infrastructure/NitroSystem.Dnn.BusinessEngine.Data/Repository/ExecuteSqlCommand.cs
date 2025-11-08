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

        public IDataReader ExecuteSqlReader(IUnitOfWork unitOfWork, CommandType commandType, string commandText, object param = null)
        {
            try
            {
                return unitOfWork.Connection.ExecuteReader(
                    sql: commandText,
                    param: param,
                    commandType: commandType,
                    transaction: unitOfWork.Transaction
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing query: {ex.Message}", ex);
            }
        }
    }
}
