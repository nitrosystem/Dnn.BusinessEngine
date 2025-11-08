using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts
{
    public interface IExportable
    {
        Task<T> Export<T>(string methodName, params object[] args) where T : class;
    }
}
