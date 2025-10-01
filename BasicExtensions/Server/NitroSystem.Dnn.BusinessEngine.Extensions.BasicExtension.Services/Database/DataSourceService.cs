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
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.StudioServices;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;
using System.Data.Common;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Tables;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Views;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeLoader;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Database
{
    public class DataSourceService : IDataSourceService
    {
        private readonly IDbConnection _connection;
        private readonly IRepositoryBase _repository;
        private readonly ITypeLoaderFactory _typeLoaderFactory;

        public DataSourceService(IDbConnection connection, IRepositoryBase repository, ITypeLoaderFactory typeLoaderFactory)
        {
            _connection = connection;
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            _repository = repository;
            _typeLoaderFactory = typeLoaderFactory;
        }

        public async Task<(IEnumerable<object> Items, int? TotalCount)> GetDataSourceService(ActionDto action, PortalSettings portalSettings)
        {
            var spParams = DbShared.FillSqlParams(action.Params);
            var data = await _repository.GetByColumnAsync<DataSourceServiceView>("ServiceId", action.ServiceId.Value);
            var type = _typeLoaderFactory.GetTypeFromAssembly(data.TypeRelativePath, data.TypeFullName, data.ScenarioName, portalSettings);

            var multi = await _connection.QueryMultipleAsync(data.StoredProcedureName, spParams,
                commandType: CommandType.StoredProcedure, commandTimeout: int.MaxValue);

            var items = await multi.ReadAsync(type);

            var totalCount = data.EnablePaging 
                ? await multi.ReadSingleAsync<int?>()
                : 0;

            return (items, totalCount);
        }
    }
}
