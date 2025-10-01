using System;
using System.Threading.Tasks;
using Dapper;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repository
{
    public class ExecuteSqlCommand: IExecuteSqlCommand
    {
        protected readonly IUnitOfWork _unitOfWork;

        public ExecuteSqlCommand(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> ExecuteSqlCommandTextAsync(string commandText, object param = null)
        {
            try
            {
                return await _unitOfWork.Connection.ExecuteAsync(
                    commandText,
                    param,
                    _unitOfWork.Transaction);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error execute query {ex.Message}", ex);
            }
        }
    }
}
