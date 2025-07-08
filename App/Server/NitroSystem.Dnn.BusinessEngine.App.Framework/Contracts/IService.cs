using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IService
    {
        void Init(IServiceWorker serviceWorker, ServiceDto service);

        Task<ServiceResult> ExecuteAsync<T>();
    }
}
