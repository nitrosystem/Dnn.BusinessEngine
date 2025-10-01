using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repository
{
    public class ExecuteSqlCommand : IExecuteSqlCommand
    {
        private readonly IUnitOfWork _unitOfWork;

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
