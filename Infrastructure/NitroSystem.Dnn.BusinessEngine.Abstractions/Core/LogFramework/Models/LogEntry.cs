using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models
{
    public class LogEntry
    {
        public Guid ScenarioId { get; set; }
        public string ScenarioName { get; set; }
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
