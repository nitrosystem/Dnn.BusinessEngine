using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Contracts
{
    public interface IServiceLocator
    {
        T GetInstance<T>(string typeName) where T : class;

        T CreateInstance<T>(string typeName, params object[] parameters) where T : class;
    }
}
