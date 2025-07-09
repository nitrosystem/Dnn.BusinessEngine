using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Mapper
{
    public static class HybridMapperWithExpressions
    {
        private static readonly Dictionary<(Type, Type), Delegate> _mappingCache = new();

        public static TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            var key = (typeof(TSource), typeof(TDestination));
            if (!_mappingCache.TryGetValue(key, out var del))
            {
                del = CreateMapExpression<TSource, TDestination>();
                _mappingCache[key] = del;
            }

            var func = (Func<TSource, TDestination>)del;
            return func(source);
        }

        private static Func<TSource, TDestination> CreateMapExpression<TSource, TDestination>()
            where TDestination : new()
        {
            var sourceParam = Expression.Parameter(typeof(TSource), "source");
            var bindings = typeof(TDestination)
                .GetProperties()
                .Where(destProp => destProp.CanWrite)
                .Select(destProp =>
                {
                    var sourceProp = typeof(TSource).GetProperty(destProp.Name);
                    if (sourceProp == null || !sourceProp.CanRead)
                        return null;

                    return Expression.Bind(
                        destProp,
                        Expression.Property(sourceParam, sourceProp)
                    );
                })
                .Where(binding => binding != null)
                .ToList();

            var body = Expression.MemberInit(
                Expression.New(typeof(TDestination)),
                bindings
            );

            var lambda = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam);
            return lambda.Compile();
        }
    }

}
