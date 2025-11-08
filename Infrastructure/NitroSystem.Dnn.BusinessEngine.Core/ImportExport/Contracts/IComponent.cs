using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ImportExport.Export;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts
{
    public interface IComponent : IDisposable
    {
        bool ContinueOnError { get; set; }

        List<string> Items { get; set; }

        List<object> Values { get; set; }

        Queue<ICustomTask> Tasks { get; set; }

        void CreateTasks();

        bool Export();

        void CompleteComponentExport(ManifestModel manifest);
    }
}
