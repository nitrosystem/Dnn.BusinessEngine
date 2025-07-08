using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Mapper
{
    public class MapperConfiguration
    {
        private readonly Dictionary<(Type Source, Type Destination), List<Action<object, object>>> _mappings = new();

        public MappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            var key = (typeof(TSource), typeof(TDestination));

            if (!_mappings.ContainsKey(key))
            {
                _mappings[key] = new List<Action<object, object>>();
            }

            return new MappingExpression<TSource, TDestination>(_mappings[key]);
        }

        public IMapper<T1, T2> CreateMapper<T1, T2>() => new CustomMapper<T1, T2>(_mappings);

    }

}
