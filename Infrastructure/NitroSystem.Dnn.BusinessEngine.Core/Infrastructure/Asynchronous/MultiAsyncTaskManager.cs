using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Asynchronous
{
    public class MultiAsyncTaskManager<TResult> : IAsyncTaskManager<TResult>
    {
        private readonly Dictionary<string, Func<Task<TResult>>> _tasks = new Dictionary<string, Func<Task<TResult>>>();
        private readonly Dictionary<string, TResult> _results = new Dictionary<string, TResult>();

        public void AddTask(string name, Func<Task<TResult>> taskFunc)
        {
            _tasks[name] = taskFunc;
        }

        public void RegisterAll(Page page)
        {
            foreach (var kvp in _tasks)
            {
                page.RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    TResult result = await kvp.Value();
                    _results[kvp.Key] = result;
                }));
            }
        }

        public TResult GetResult(string name)
        {
            return _results.TryGetValue(name, out var result) ? result : default;
        }

        public IReadOnlyDictionary<string, TResult> AllResults => _results;
    }
}
