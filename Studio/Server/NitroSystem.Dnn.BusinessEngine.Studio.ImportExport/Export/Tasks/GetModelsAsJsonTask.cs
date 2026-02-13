using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Tasks
{
    public class GetModelsAsJsonTask<T> : ICustomTask where T : class
    {
        private readonly IExportable _service;
        private readonly string _methodName;
        private readonly object[] _params;
        private readonly Action<T> _action;

        public string Name { get; set; }
        public bool ContinueOnError { get; set; }

        public GetModelsAsJsonTask(IExportable service, string methodName, params object[] args)
        {
            _service = service;
            _methodName = methodName;
            _params = args;

            Name = methodName;
        }

        public GetModelsAsJsonTask(IExportable service, string methodName, Action<T> action, params object[] args)
        {
            _service = service;
            _methodName = methodName;
            _action = action;
            _params = args;

            Name = methodName;
        }

        public async Task<TaskResult> ExecuteAsync()
        {
            var result = new TaskResult();
            var data = await _service.Export<T>(_methodName, _params);

            if (_action != null)
                _action.Invoke(data);

            var json = JsonConvert.SerializeObject(data);

            result.Data = json;
            result.IsSuccess = true;

            return result;
        }
    }
}
