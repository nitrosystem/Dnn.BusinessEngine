using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.DatabaseEntities.Views;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services
{
    public class CustomQueryService
    {
        private readonly ISql _sql;
        private readonly ITypeLoaderFactory _typeLoaderFactory;

        public CustomQueryService(IDbConnection connection, ISql sql, ITypeLoaderFactory typeLoaderFactory)
        {
            _sql = sql;
            _typeLoaderFactory = typeLoaderFactory;
        }

        public async Task<object> ExecuteCustomQuery(Guid serviceId, IReadOnlyList<ParamInfo> filledParams)
        {
            var data = await _sql.GetByColumnAsync<CustomQueryServiceView>("ServiceId", serviceId);
            var spName = data.StoredProcedureName;
            var spParams = ParamHelper.FillSqlParamsAsObject(filledParams);

            if ((ResultType)data.ResultType == ResultType.None)
                await _sql.ExecuteStoredProcedureAsync(spName, spParams);
            else if ((ResultType)data.ResultType == ResultType.Scaler)
                return await _sql.ExecuteStoredProcedureScalerAsync<object>(spName, "", spParams);
            else if ((ResultType)data.ResultType == ResultType.Row || (ResultType)data.ResultType == ResultType.List)
            {
                if (string.IsNullOrEmpty(data.TypeRelativePath))
                    throw new ArgumentNullException("TypeRelativePath", "Type Relative Path is null.");

                if (string.IsNullOrEmpty(data.TypeFullName))
                    throw new ArgumentNullException("TypeFullName", "Type FullName is null.");

                var type = _typeLoaderFactory.GetTypeFromAssembly(data.TypeRelativePath, data.TypeFullName);

                if ((ResultType)data.ResultType == ResultType.Row)
                    return await _sql.ExecuteStoredProcedureAsync(type, spName, "", spParams);
                else if ((ResultType)data.ResultType == ResultType.List)
                    return await _sql.ExecuteStoredProcedureAsListAsync(type, spName, "", spParams);
            }

            return null;
        }
    }
}
