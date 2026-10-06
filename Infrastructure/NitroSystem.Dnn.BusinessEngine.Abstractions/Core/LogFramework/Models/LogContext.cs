using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models
{
    public class LogContext
    {
        public Guid TraceId { get; set; } = Guid.NewGuid();
        public string User { get; set; }
        public string CorrelationId { get; set; }
        public Dictionary<string, object> Data { get; } = new();
    }
}
