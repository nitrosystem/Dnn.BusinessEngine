using System;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public LogLevel Level { get; set; }
        public string Source { get; set; }   // مثلاً: "TaskEngine", "GCMonitor", "Worker"
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
