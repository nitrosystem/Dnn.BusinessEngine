using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework
{
    public class EventNodeBaseImpl : EventNodeBase
    {
        private readonly Func<EventContext, Task> _exec;
        public EventNodeBaseImpl(Func<EventContext, Task> exec, string name = "Exec") : base(name) => _exec = exec;
        public override Task ExecuteAsync(EventContext context) => _exec(context);
    }
}
