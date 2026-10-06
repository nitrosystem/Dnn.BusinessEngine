using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Asynchronous
{
    public interface IAsyncTaskManager<TResult>
    {
        void AddTask(string name, Func<Task<TResult>> taskFunc);
        void RegisterAll(Page page);
        TResult GetResult(string name);
        IReadOnlyDictionary<string, TResult> AllResults { get; }
    }
}
