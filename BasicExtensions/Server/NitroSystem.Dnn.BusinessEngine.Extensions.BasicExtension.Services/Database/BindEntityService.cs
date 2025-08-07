using Dapper;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Database;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DB.Entities;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Database
{
    public class BindEntityService : IBindEntityService
    {
        private readonly IDbConnection _connection;
        private readonly IRepositoryBase _repository;

        public BindEntityService(IDbConnection connection, IRepositoryBase repository)
        {
            _connection = connection;
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            _repository = repository;
        }

        public async Task<object> GetBindEntityService(ActionDto action)
        {
            var spParams = DbShared.FillSqlParams(action.Params);

            var spName = await _repository.GetColumnValueAsync<BindEntityServiceInfo, string>("StoredProcedureName", "ServiceId", action.ServiceId);

            return await _connection.QuerySingleAsync(spName, spParams, commandType: CommandType.StoredProcedure, commandTimeout: int.MaxValue);
        }
    }
}
