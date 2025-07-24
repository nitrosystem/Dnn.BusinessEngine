using NitroSystem.Dnn.BusinessEngine.App.Services.Dto.Action;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IServiceWorker
    {
        Task<ServiceResult> RunServiceByAction<T>(ActionDto action);

        //Task<ServiceResult> RunService<T>(Guid serviceID, IEnumerable<IParamInfo> postedParams);

        IService CreateInstance(string serviceType);

        //DynamicParameters FillSqlParams(IEnumerable<IParamInfo> postedParams);
    }
}
