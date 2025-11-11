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
        /// Implement the work. Should honor cancellation and report progress.
        Task RunAsync(CancellationToken token, IProgress<ProgressInfo> progress);
    }
}
