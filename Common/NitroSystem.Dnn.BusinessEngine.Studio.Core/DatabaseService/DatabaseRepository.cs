using Dapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.DatabaseService
{
    public class DatabaseRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public DatabaseRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void DeleteEntitiesRow<T>(IEnumerable<string> tables, string columnName, T id)
        {
            foreach (var tableName in tables)
            {
                DeleteEntityRow<T>(tableName, columnName, id);
            }
        }

        public void DeleteEntityRow<T>(string tableName, string columnName, T id)
        {
            string query = string.Format("DELETE FROM {0} WHERE {1} = @value", tableName, columnName);
            _unitOfWork.Connection.Execute(query, new { value = id }, _unitOfWork.Transaction);
        }
    }

}
