using Dapper;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Services.DB.Repositories;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using NitroSystem.Dnn.BusinessEngine.Framework.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Services.Database
{
    public class SubmitEntityService : ServiceBase<object>, IService
    {
        public override async Task<ServiceResult> ExecuteAsync<T>()
        {
            ServiceResult result = new ServiceResult();

            try
            {
                var connection = new System.Data.SqlClient.SqlConnection(DotNetNuke.Data.DataProvider.Instance().ConnectionString);

                var dbParams = this.ServiceWorker.FillSqlParams(this.Service.Params);

                foreach (var item in dbParams.ParameterNames)
                {
                    result.Query += item + " = " + dbParams.Get<string>(item) + " ";
                }

                var submitEntityService = SubmitEntityServiceRepository.GetSubmitEntityService(this.Service.ServiceID);

                if (this.Service.HasResult && this.Service.ResultType == Framework.Enums.ServiceResultType.List)
                {
                    var data = await SqlMapper.QueryAsync(connection, "[dbo]." + submitEntityService.StoredProcedureName, dbParams, commandTimeout: int.MaxValue, commandType: CommandType.StoredProcedure);
                    result.DataList = JArray.FromObject(data).ToObject<JArray>();
                }
                else if (this.Service.HasResult && this.Service.ResultType == Framework.Enums.ServiceResultType.DataRow)
                {
                    var data = await SqlMapper.QueryAsync(connection, "[dbo]." + submitEntityService.StoredProcedureName, dbParams, commandTimeout: int.MaxValue, commandType: CommandType.StoredProcedure);
                    result = data != null && data.Any() ? JObject.FromObject(data.First()) : default(T);
                }
                else if (this.Service.HasResult && this.Service.ResultType == Framework.Enums.ServiceResultType.Scaler)
                {
                    var data = await SqlMapper.ExecuteScalarAsync(connection, "[dbo]." + submitEntityService.StoredProcedureName, dbParams, commandTimeout: int.MaxValue, commandType: CommandType.StoredProcedure);
                    result.Data = new JValue(data);
                }
                else
                {
                    await SqlMapper.ExecuteAsync(connection, "[dbo]." + submitEntityService.StoredProcedureName, dbParams, commandTimeout: int.MaxValue, commandType: CommandType.StoredProcedure);
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
