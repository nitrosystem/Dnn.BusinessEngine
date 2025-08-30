using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class EntityAttribute : Attribute
    {
        public IEntity Entity { get; }

        public EntityAttribute(Type type)
        {
            this.Entity = Activator.CreateInstance(type) as IEntity;
        }
    }
}
