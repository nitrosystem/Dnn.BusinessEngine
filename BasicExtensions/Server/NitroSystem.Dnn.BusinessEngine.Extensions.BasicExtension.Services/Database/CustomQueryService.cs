using Dapper;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Services.DB.Repositories;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Dto;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Services.Database
{
    public class CustomQueryService : ServiceBase<object>, IService
    {
        public async override Task<ServiceResult> ExecuteAsync<T>()
        {
            ServiceResult result = new ServiceResult();

            var connection = new System.Data.SqlClient.SqlConnection(DotNetNuke.Data.DataProvider.Instance().ConnectionString);

            try
            {
                var dbParams = this.ServiceWorker.FillSqlParams(this.Service.Params);

                foreach (var item in dbParams.ParameterNames)
                {
                    result.Query += item + " = " + dbParams.Get<string>(item) + " ";
                }

                var customQuery = CustomQueryServiceRepository.GetCustomQueryService(this.Service.ServiceID);

                if (this.Service.HasResult && this.Service.ResultType == Framework.Enums.ServiceResultType.List)
                {
                    result.ResultType = Framework.Enums.ServiceResultType.List;

                    var data = SqlMapper.QueryAsync(connection, "[dbo]." + customQuery.StoredProcedureName, dbParams, commandTimeout: int.MaxValue, commandType: CommandType.StoredProcedure).GetAwaiter().GetResult();
                    result.DataList = JArray.FromObject(data).ToObject<JArray>();
                    result.TotalCount = 0;
                    if (data.Any())
                    {
                        var firstItem = result.DataList.First();
                        result.TotalCount = firstItem["bEngine_TotalCount"] != null ? firstItem["bEngine_TotalCount"].Value<long>() : 0;
                    }
                }
                else if (this.Service.HasResult && this.Service.ResultType == Framework.Enums.ServiceResultType.DataRow)
                {
                    result.ResultType = Framework.Enums.ServiceResultType.DataRow;

                    var data =  SqlMapper.QueryAsync(connection, "[dbo]." + customQuery.StoredProcedureName, dbParams, commandTimeout: int.MaxValue, commandType: CommandType.StoredProcedure).GetAwaiter().GetResult();
                    result.DataRow = data != null && data.Any() ? JObject.FromObject(data.First()) : default(T);
                }
                else if (this.Service.HasResult && this.Service.ResultType == Framework.Enums.ServiceResultType.Scaler)
                {
                    result.ResultType = Framework.Enums.ServiceResultType.Scaler;

                    var data = await SqlMapper.ExecuteScalarAsync(connection, "[dbo]." + customQuery.StoredProcedureName, dbParams, commandTimeout: int.MaxValue, commandType: CommandType.StoredProcedure);
                    result.Data = data;
                }
                else
                {
                    await SqlMapper.ExecuteAsync(connection, "[dbo]." + customQuery.StoredProcedureName, dbParams, commandTimeout: int.MaxValue, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.ErrorException = ex;
            }

            //return (T)Convert.ChangeType(result, typeof(T));
            return result;
        }

        public override bool TryParseModel(string serviceSettings)
        {
            throw new NotImplementedException();
        }
    }
}
