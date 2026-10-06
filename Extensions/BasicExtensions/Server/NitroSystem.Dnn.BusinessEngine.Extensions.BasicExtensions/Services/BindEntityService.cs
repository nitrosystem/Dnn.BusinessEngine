using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.DatabaseEntities.Views;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services
{
    public class BindEntityService
    {
        private readonly ISql _sql;
        private readonly ITypeLoaderFactory _typeLoaderFactory;

        public BindEntityService(IDbConnection connection, ISql sql, ITypeLoaderFactory typeLoaderFactory)
        {
            _sql = sql;
            _typeLoaderFactory = typeLoaderFactory;
        }

        public async Task<object> GetBindEntityService(Guid serviceId, IReadOnlyList<ParamInfo> filledParams)
        {
            var data = await _sql.GetByColumnAsync<BindEntityServiceView>("ServiceId", serviceId);
            var type = _typeLoaderFactory.GetTypeFromAssembly(data.TypeRelativePath, data.TypeFullName);
            var spName = data.StoredProcedureName;
            var spParams = ParamHelper.FillSqlParamsAsObject(filledParams);

            var result = await _sql.ExecuteStoredProcedureAsync(type, spName, "", spParams);
            return result;
        }
    }
}
