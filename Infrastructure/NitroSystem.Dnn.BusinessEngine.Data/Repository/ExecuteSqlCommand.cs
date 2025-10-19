using System;
using System.Threading.Tasks;
using Dapper;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repository
{
    public class ExecuteSqlCommand : IExecuteSqlCommand
    {
        public async Task<int> ExecuteSqlCommandTextAsync(IUnitOfWork unitOfWork, string commandText, object param = null)
        {
            try
            {
                return await unitOfWork.Connection.ExecuteAsync(
                    commandText,
                    param,
                    unitOfWork.Transaction);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error execute query {ex.Message}", ex);
            }
        }
    }
}
