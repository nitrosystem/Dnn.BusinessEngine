using System;
using System.Linq;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Export.Tasks
{
    public class ZipTask : ICustomTask
    {
        private readonly IComponent _component;
        private readonly string _zipPath;
        private readonly string _sourcePath;
        private readonly string _filename;

        public bool ContinueOnError { get; set; }

        Func<string, string, bool, string> zipFunc = ZipProvider.Zip;

        public ZipTask(IComponent component, string zipPath, string filename, string manifestPropertyName, string sourcePath)
        {
            _component = component;
            _sourcePath = sourcePath;
            _zipPath = zipPath;
            _filename = filename;

            component.Items.Add(manifestPropertyName);
        }

        public void Zip()
        {
            this.zipFunc(_zipPath + _filename, _sourcePath, true);

            _component.Values.Add(_filename);
        }

        public async Task Start()
        {
            await Task.Yield();

            Zip();
        }
    }
}
