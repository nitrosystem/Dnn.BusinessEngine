using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader
{
    public sealed class TypeDescriptor
    {
        public Type Type { get; }
        public Dictionary<string, Func<object, object>> Getters { get; }

        public TypeDescriptor(Type type)
        {
            Type = type;
            Getters = type.GetProperties()
                          .ToDictionary(p => p.Name,
                                        p => (Func<object, object>)BuildGetter(p));
        }

        private static Func<object, object> BuildGetter(PropertyInfo prop)
        {
            var param = Expression.Parameter(typeof(object), "x");
            var body = Expression.Convert(
                Expression.Property(Expression.Convert(param, prop.DeclaringType), prop),
                typeof(object));
            return Expression.Lambda<Func<object, object>>(body, param).Compile();
        }
    }
}
