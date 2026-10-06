using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.DatabaseEntities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services
{
    public class SubmitEntityService  
    {
        private readonly ICacheService _cacheService;
        private readonly ISql _sql;

        public SubmitEntityService(ICacheService cacheService, ISql sql)
        {
            _cacheService = cacheService;
            _sql = sql;
        }

        public async Task<object> SaveEntityRow(Guid serviceId, IReadOnlyList<ParamInfo> filledParams)
        {
            var data = await _sql.GetByColumnAsync<SubmitEntityServiceInfo>("ServiceId", serviceId);
            var spName = data.StoredProcedureName;
            var spParams = ParamHelper.FillSqlParamsAsObject(filledParams);

            var result = await _sql.ExecuteStoredProcedureScalerAsync<object>(data.StoredProcedureName, "", spParams);
            return result;
        }
    }
}
