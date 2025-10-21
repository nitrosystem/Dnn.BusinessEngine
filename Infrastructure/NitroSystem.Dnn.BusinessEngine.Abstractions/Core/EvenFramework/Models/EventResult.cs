using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EvenFramework.Models
{
    public class EventResult
    {
        public bool Success { get; set; }
        public TimeSpan Duration { get; set; }
        public long MemoryUsed { get; set; }
        public string ErrorMessage { get; set; }
    }
}
