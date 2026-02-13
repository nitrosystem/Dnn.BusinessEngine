using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Events;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ImportExport.Enums;
using DotNetNuke.Entities.Portals;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export
{
    public class ExportJob : IJob
    {
        private readonly ExportScope _scope;
        private readonly List<Component> _components = new List<Component>();

        public ExportJob(ExportScope scope, string name)
        {
            _scope = scope;

            Name = name;
        }

        public string JobId { get; } = Guid.NewGuid().ToString();
        public string Name { get; }

        public event ExportProgressHandler OnProgress;
        public event ExportCompletedHandler OnExportCompleted;

        public void RegisterComponents(params Component[] components)
        {
            var p = PortalSettings.Current;

            foreach (var component in components)
            {
                _components.Add(component);
            }
        }

        //public async Task  CreateWorkflow()
        //{
        //    foreach (var component in _components)
        //    {
        //        component.CreateTasks();
        //    }

        //    foreach (var component in _components)
        //    {
        //        var isSuccess = await component.ExportComponentAsync();
        //        if (!isSuccess) throw new Exception();
        //    }
        //}

        public async Task RunAsync(CancellationToken token)
        {
            var sb = new StringBuilder();

            foreach (var component in _components)
            {
                try
                {
                    var p = PortalSettings.Current;

                    var exportedData = await component.Service.Export(_scope, component.Params);
                    var json = JsonConvert.SerializeObject(exportedData);
                    sb.AppendLine(json);

                    component.IsSuccess = true;

                    OnProgress?.Invoke(component.Name, component.IsSuccess, JobId);
                }
                catch (Exception ex)
                {
                    if (component.ContinueOnError) continue;
                    else throw ex;
                }
            }

            OnExportCompleted?.Invoke(_scope, JobId, Name, sb.ToString());
        }
    }
}

//public void Init(Action<int> reportProgress)
//{
//    if (Directory.Exists(_manifestPath)) Directory.Delete(_manifestPath, true);
//    Directory.CreateDirectory(_manifestPath);

//    var manager = new BackgroundJobManager<ExportFramework>(
//        _manifestPath,
//        this,
//        (context, progress, worker) => context.Work(progress),
//        reportProgress
//    );

//    manager.OnError += msg => Console.WriteLine("[ERROR] " + msg);
//    manager.OnCompleted += () => Console.WriteLine("Export completed successfully.");

//    manager.Start();
//}