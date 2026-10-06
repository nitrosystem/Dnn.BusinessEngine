using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.DatabaseEntities.Views;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services
{
    public class DataSourceService 
    {
        private readonly ISql _sql;
        private readonly ITypeLoaderFactory _typeLoaderFactory;

        public DataSourceService(ISql sql, ITypeLoaderFactory typeLoaderFactory)
        {
            _sql = sql;
            _typeLoaderFactory = typeLoaderFactory;
        }

        public async Task<(IEnumerable<object> Items, int TotalCount)> GetDataSourceService(
            Guid serviceId,
            IReadOnlyList<ParamInfo> serviceParams)
        {
            var data = await _sql.GetByColumnAsync<DataSourceServiceView>("ServiceId", serviceId);
            var type = _typeLoaderFactory.GetTypeFromAssembly(data.TypeRelativePath, data.TypeFullName);
            var spName = data.StoredProcedureName;
            var spParams = ParamHelper.FillSqlParamsAsObject(serviceParams);

            if (data.EnablePaging)
            {
                var results = await _sql.ExecuteStoredProcedureForPagingAsync(type, spName, "", spParams);
                return (results.Items, results.TotalCount);
            }
            else
            {
                var result = await _sql.ExecuteStoredProcedureAsListAsync(type, spName, "", spParams);
                return (result, result.Count());
            }
        }
    }
}
