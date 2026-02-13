using System;
using System.Threading;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob.Models
{
    public sealed class RunningJobInfo
    {
        public JobContext Request { get; set; }
        public DateTime StartedAt { get; set; }
        public CancellationTokenSource CancellationSource { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsCancelled { get; set; }
    }
}
