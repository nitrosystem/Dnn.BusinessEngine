using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework
{
    public abstract class EventNodeBase : IEventNode
    {
        public virtual string Name { get; }

        protected EventNodeBase(string name) => Name = name;

        public abstract Task ExecuteAsync(EventContext context);
    }
}
