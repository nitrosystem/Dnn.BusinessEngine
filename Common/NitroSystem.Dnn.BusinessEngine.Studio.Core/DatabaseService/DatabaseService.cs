using DotNetNuke.Collections;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users.Social;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.DatabaseService
{
    public class DatabaseService
    {
        private readonly IDbConnection _connection;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DatabaseRepository _repository;

        public DatabaseService(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _unitOfWork = new UnitOfWork(_connection);
            _repository = new DatabaseRepository(_unitOfWork);
        }

        

        
    }
}
