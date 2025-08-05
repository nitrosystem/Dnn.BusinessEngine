using Dapper;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Services.DB.Repositories;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Dto;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Services.Database
{
    public class BindEntityService : ServiceBase<object>, IService
    {
        public async override Task<ServiceResult> ExecuteAsync<T>()
        {
            ServiceResult result = new ServiceResult() { ResultType = this.Service.ResultType };

            try
            {
                var connection = new System.Data.SqlClient.SqlConnection(DotNetNuke.Data.DataProvider.Instance().ConnectionString);

                var dbParams = this.ServiceWorker.FillSqlParams(this.Service.Params);

                var dataSourceService = BindEntityServiceRepository.GetBindEntityService(this.Service.ServiceID);

                if (dataSourceService == null) throw new Exception(string.Format("Bind Entity service({0}|{1}) is null!", this.Service.ServiceName, this.Service.ServiceID));

                var spParams = new List<string>();
                foreach (var item in dbParams.ParameterNames)
                {
                    var value = dbParams.Get<object>(item);
                    spParams.Add(string.Format("@{0} = {1}", item, value == null ? "NULL" : value.ToString()));
                }
                result.Query = string.Format("exec {0} {1}", dataSourceService.StoredProcedureName, string.Join(",", spParams));

                var data = SqlMapper.Query(connection, "[dbo]." + dataSourceService.StoredProcedureName, dbParams, commandTimeout: int.MaxValue, commandType: CommandType.StoredProcedure);

                if (this.Service.ResultType == Framework.Enums.ServiceResultType.DataRow)
                {
                    result.DataRow = data != null && data.Any() ? JObject.FromObject(data.First()) : null;
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.ErrorException = ex;
            }

            return result;
        }

        public override bool TryParseModel(string serviceSettings)
        {
            throw new NotImplementedException();
        }
    }
}
