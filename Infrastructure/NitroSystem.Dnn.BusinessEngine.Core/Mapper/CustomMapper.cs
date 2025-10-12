using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Mapper
{
    public class CustomMapper<TSource, TDestination> : IMapper<TSource, TDestination>
    {
        private readonly List<Action<object, object>> _actions;

        public CustomMapper(Dictionary<(Type, Type), List<Action<object, object>>> mappings)
        {
            var key = (typeof(TSource), typeof(TDestination));

            if (!mappings.TryGetValue(key, out _actions))
            {
                throw new InvalidOperationException($"Mapping not found from {typeof(TSource).Name} to {typeof(TDestination).Name}");
            }
        }

        public TDestination Map(TSource source)
        {
            var destination = Activator.CreateInstance<TDestination>();

            foreach (var action in _actions)
            {
                action(source, destination);
            }

            return destination;
        }
    }
}
