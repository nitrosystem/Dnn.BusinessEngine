using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export
{
    public class ExportFramework
    {
        private readonly List<IComponent> _components = new List<IComponent>();
        private readonly ManifestModel _manifestModel;
        private readonly string _manifestPath;
        private static Thread worker;

        public ExportFramework(string manifestPath, ManifestModel manifestModel)
        {
            _manifestPath = manifestPath;
            _manifestModel = manifestModel;
        }

        Func<string, string, bool, bool> createFileFunc = FileUtil.CreateTextFile;
        Func<string, string, bool, string> zipFunc = ZipProvider.Zip;

        public delegate void Worker(Action<int> reportProgress);

        public void InitTemp(Worker work, Action<int> reportProgress)
        {
            worker = new Thread(() => work(reportProgress));
            worker.IsBackground = true;
            worker.Start();
        }

        public void Init(Worker work, Action<int> reportProgress)
        {
            if (Directory.Exists(_manifestPath)) Directory.Delete(_manifestPath, true);
            Directory.CreateDirectory(_manifestPath);

            var manager = new BackgroundJobManager<ExportFramework>(
                _manifestPath,
                this,
                (context, progress, worker) => context.Work(progress),
                reportProgress
            );

            manager.OnError += msg => Console.WriteLine("[ERROR] " + msg);
            manager.OnCompleted += () => Console.WriteLine("Export completed successfully.");

            manager.Start();
        }

        public void Work(Action<int> reportProgress)
        {
            int inedx = 0;
            foreach (var component in _components)
            {
                using (var cmp = component)
                {
                    var isComplete = cmp.Export();
                    if (isComplete) cmp.CompleteComponentExport(_manifestModel);

                    reportProgress(++inedx * 10);
                }
            }

            this.CompleteExport();
        }

        public void ProgressChanged(int index)
        {
            Console.WriteLine(index);
        }

        public void RegisterComponent(IComponent component)
        {
            _components.Add(component);
        }

        public void CreateWorkflow()
        {
            foreach (var component in _components)
            {
                component.CreateTasks();
            }
        }

        public void CompleteExport()
        {
            string json = JsonConvert.SerializeObject(_manifestModel);
            string filename = _manifestPath + "_manifest.json";
            this.createFileFunc(filename, json, true);

            string targetPath = Path.GetDirectoryName(_manifestPath);
            string result = this.zipFunc(targetPath + ".zip", targetPath, true);

            Directory.Delete(_manifestPath, true);
        }
    }
}
