using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models
{
    public class LogEntry
    {
        public string Message { get; set; }
        public LogLevel Level { get; set; }
        public string Source { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string User { get; set; }
        public string CorrelationId { get; set; }
        public Exception Exception { get; set; }
    }
}
