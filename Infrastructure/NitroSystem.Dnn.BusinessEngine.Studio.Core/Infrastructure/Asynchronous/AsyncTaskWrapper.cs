using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Asynchronous
{
    public class AsyncTaskWrapper<TResult>
    {
        private readonly string _taskName;
        private readonly Func<Task<TResult>> _taskFunc;
        private TResult _result;

        public AsyncTaskWrapper(string taskName, Func<Task<TResult>> taskFunc)
        {
            _taskName = taskName;
            _taskFunc = taskFunc;
        }

        public void Register(Page page)
        {
            page.RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                _result = await _taskFunc();
            }));
        }

        public TResult Result => _result;
    }
}