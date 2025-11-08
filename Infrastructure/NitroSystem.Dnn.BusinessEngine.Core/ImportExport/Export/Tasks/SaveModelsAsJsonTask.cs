using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Base;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export.Tasks
{
    public class SaveModelsAsJsonTask : ICustomTask
    {
        private readonly IComponent _component;
        private readonly IExportable _service;
        private readonly string _methodName;
        private readonly string _manifestPath;
        private readonly string _filename;
        private readonly object[] _params;

        public bool ContinueOnError { get; set; }

        Func<string, string, bool, bool> createFile = FileUtil.CreateTextFile;

        public SaveModelsAsJsonTask(IComponent component, IExportable service, string methodName, string manifestPath, string manifestPropertyName, string filename, params object[] args)
        {
            _component = component;
            _service = service;
            _methodName = methodName;
            _manifestPath = manifestPath;
            _filename = filename;
            _params = args;

            component.Items.Add(manifestPropertyName);
        }

        public async Task<string> Get()
        {
            var data = await _service.Export<ScenarioViewModel>(_methodName, _params);
            var result = JsonConvert.SerializeObject(data);
            return result;
        }

        public async Task Start()
        {
            var json = await Get();
            this.createFile(_manifestPath + _filename, json, true);

            _component.Values.Add(_filename);
        }
    }
}
