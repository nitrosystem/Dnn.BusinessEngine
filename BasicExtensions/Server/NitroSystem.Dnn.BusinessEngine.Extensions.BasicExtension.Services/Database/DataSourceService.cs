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
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DB.Entities;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Database
{
    public class DataSourceService : IDataSourceService
    {
        private readonly IDbConnection _connection;
        private readonly IRepositoryBase _repository;

        public DataSourceService(IDbConnection connection, IRepositoryBase repository)
        {
            _connection = connection;
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            _repository = repository;
        }

        public async Task<DataSourceResultDto> GetDataSourceService(ActionDto action)
        {
            var spParams = DbShared.FillSqlParams(action.Params);

            var spName = await _repository.GetColumnValueAsync<DataSourceServiceInfo, string>("StoredProcedureName", "ServiceId", action.ServiceId);

            var data = await _connection.QueryAsync(spName, spParams, commandType: CommandType.StoredProcedure, commandTimeout: int.MaxValue);

            //result.Query = string.Format("exec {0} {1}", dataSourceService.StoredProcedureName, string.Join(",", spParams));

            //var data = SqlMapper.Query(connection, "[dbo]." + dataSourceService.StoredProcedureName, spParams, );

            //if (this.Service.ResultType == Framework.Enums.ServiceResultType.DataRow)
            //{
            //    result.DataRow = data != null && data.Any() ? JObject.FromObject(data.First()) : null;
            //}
            //else if (this.Service.ResultType == Framework.Enums.ServiceResultType.List)
            //{
            //    result.DataList = JArray.FromObject(data).ToObject<JArray>();
            //    result.;
            //}

            return new DataSourceResultDto()
            {
                Items = data,
                //TotalCount = data.Any() ? data.First().bEngine_TotalCount : 0
            };
        }
    }
}
