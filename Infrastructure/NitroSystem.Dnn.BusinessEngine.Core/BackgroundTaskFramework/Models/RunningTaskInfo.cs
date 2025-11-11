using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models
{
    public class RunningTaskInfo
    {
        public BackgroundTaskRequest Request { get; set; }
        public CancellationTokenSource CancellationSource { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public Task RunningTask { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCancelled { get; set; }
    }

}
