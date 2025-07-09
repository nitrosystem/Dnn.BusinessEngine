using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NitroSystem.Dnn.BusinessEngine.Core.Mapper
{
    public class ExpressionMapper<TSource, TDestination>
    {
        private readonly List<CustomMapping<TSource, TDestination>> _customMappings = new();

        public void AddCustomMapping<TSourceProp, TDestProp>(
                Expression<Func<TSource, TSourceProp>> sourceSelector,
                Expression<Func<TDestination, TDestProp>> destinationSelector,
                Func<TSource, object> mapFunc, Func<TSource, bool> preCondition = null)
        {
            var sourceProperty = GetPropertyInfo(sourceSelector);
            var destinationProperty = GetPropertyInfo(destinationSelector);

            _customMappings.Add(new CustomMapping<TSource, TDestination>
            {
                SourceProperty = sourceProperty,
                DestinationProperty = destinationProperty,
                MapFunc = mapFunc,
                PreCondition = preCondition
            });
        }


        public TDestination Map(TSource source)
        {
            if (source == null) return default(TDestination);

            var destination = Activator.CreateInstance<TDestination>();

            foreach (var destProp in typeof(TDestination).GetProperties())
            {
                var customMapping = _customMappings.FirstOrDefault(m => m.DestinationProperty.Name == destProp.Name);
                if (customMapping != null && (customMapping.PreCondition == null || customMapping.PreCondition(source)))
                {
                    var mappedValue = customMapping.MapFunc(source);
                    destProp.SetValue(destination, mappedValue);
                }
                else if (customMapping == null)
                {
                    var sourceProp = typeof(TSource).GetProperty(destProp.Name);
                    if (sourceProp != null)
                    {
                        var value = sourceProp.GetValue(source);
                        destProp.SetValue(destination, value);
                    }
                }
            }

            return destination;
        }

        private PropertyInfo GetPropertyInfo<T, TProp>(Expression<Func<T, TProp>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
                return memberExpression.Member as PropertyInfo;

            if (expression.Body is UnaryExpression unaryExpression &&
                unaryExpression.Operand is MemberExpression operandMemberExpression)
                return operandMemberExpression.Member as PropertyInfo;

            return null;
        }

        private class CustomMapping<TS, TD>
        {
            public PropertyInfo SourceProperty { get; set; }
            public PropertyInfo DestinationProperty { get; set; }
            public Func<TS, object> MapFunc { get; set; }
            public Func<TS, bool> PreCondition { get; set; }
        }
    }
}
