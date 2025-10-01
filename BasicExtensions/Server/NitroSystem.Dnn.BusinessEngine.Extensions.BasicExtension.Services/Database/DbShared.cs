using System.Linq;
using System.Collections.Generic;
using Dapper;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Database
{
    internal static class DbShared
    {
        public static DynamicParameters FillSqlParams(IEnumerable<ActionParamDto> serviceParams)
        {
            var result = new DynamicParameters();

            foreach (var param in serviceParams ?? Enumerable.Empty<ActionParamDto>())
            {
                result.Add(param.ParamName, param.ParamValue);
            }

            return result;
        }
    }
}
