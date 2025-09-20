using Dapper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Database
{
    public class SubmitEntityService : ISubmitEntityService
    {
        private readonly IDbConnection _connection;
        private readonly IRepositoryBase _repository;

        public SubmitEntityService(IDbConnection connection, IRepositoryBase repository)
        {
            _connection = connection;
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            _repository = repository;
        }

        public async Task<object> SaveEntityRowAsync(ActionDto action, PortalSettings portalSettings)
        {
            var spParams = DbShared.FillSqlParams(action.Params);
            var storedProcedureName = await _repository.GetColumnValueAsync<SubmitEntityServiceInfo, string>("StoredProcedureName", "ServiceId", action.ServiceId.Value);

            var result = await _connection.ExecuteScalarAsync(storedProcedureName, spParams,
                commandType: CommandType.StoredProcedure, commandTimeout: int.MaxValue);

            return result;
        }

        public object SaveEntityRow(ActionDto action, PortalSettings portalSettings)
        {
            var spParams = DbShared.FillSqlParams(action.Params);
            var storedProcedureName = _repository.GetColumnValue<SubmitEntityServiceInfo, string>("StoredProcedureName", "ServiceId", action.ServiceId.Value);

            var result = _connection.ExecuteScalar(storedProcedureName, spParams,
                commandType: CommandType.StoredProcedure, commandTimeout: int.MaxValue);

            return result;
        }
    }
}
