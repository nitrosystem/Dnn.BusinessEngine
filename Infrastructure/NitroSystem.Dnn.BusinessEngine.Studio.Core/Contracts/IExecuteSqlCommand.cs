using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Contracts
{
    public interface IExecuteSqlCommand
    {
        Task<int> ExecuteSqlCommandTextAsync(string commandText, object param = null);
    }
}
