using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Models
{
    public class EventNode : IDisposable
    {
        public string Name { get; }
        public DateTime Start { get; } = DateTime.UtcNow;
        public DateTime? End { get; private set; }
        public Exception Exception { get; private set; }

        public EventNode(string name) => Name = name;

        public void Success(string message = null)
        {
            // optional: add message to logs
        }

        public void Error(Exception ex)
        {
            Exception = ex;
        }

        public void Dispose()
        {
            End = DateTime.UtcNow;
            // push to EventContext log list or PushingServer
        }
    }
}
