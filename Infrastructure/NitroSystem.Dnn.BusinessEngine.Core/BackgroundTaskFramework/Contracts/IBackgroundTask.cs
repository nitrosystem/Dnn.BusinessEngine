using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Contracts
{
    public interface IBackgroundTask
    {
        string TaskId { get; }
        string Name { get; }
        Task RunAsync(CancellationToken token, IProgress<ProgressInfo> progress);
    }
}
