using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Models
{
    public class EventLog
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Category { get; }      // e.g., "Info", "Error", "Warning", "Metric"
        public string Message { get; }
        public string Source { get; }        // component name / node id
        public Exception Exception { get; }  // optional

        public EventLog(string category, string message, string source = null, Exception ex = null)
        {
            Category = category;
            Message = message;
            Source = source;
            Exception = ex;
        }
    }

}
