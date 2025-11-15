using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Models
{
    public class MetricSnapshot
    {
        public Guid EventId { get; set; }
        public string NodeName { get; set; } // null for event-level
        public Dictionary<string, double> Metrics { get; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
