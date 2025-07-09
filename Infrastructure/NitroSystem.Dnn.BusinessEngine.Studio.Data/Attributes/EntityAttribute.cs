using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes
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
