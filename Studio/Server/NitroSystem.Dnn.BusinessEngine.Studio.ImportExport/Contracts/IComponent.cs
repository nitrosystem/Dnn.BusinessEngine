using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Events;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Contracts
{
    public interface IComponent : IDisposable
    {
        string Name { get; set; }
        string Data { get; set; }
        IDictionary<string, string> Resources { get; set; }
        Queue<ICustomTask> Tasks { get; set; }

        void CreateTasks();
        Task<bool> ExportComponentAsync();

        event ExportProgressHandler OnProgress;
    }
}
