using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Utils
{
    public static class ReflectionUtil
    {
        public static TDestination PropertyCopier<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            TDestination destination = new TDestination();

            var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var sourceProp in sourceProperties)
            {
                if (!sourceProp.CanRead)
                    continue;

                var targetProp = destinationProperties.FirstOrDefault(p => p.Name == sourceProp.Name && p.CanWrite);

                if (targetProp != null && targetProp.PropertyType == sourceProp.PropertyType)
                {
                    var value = sourceProp.GetValue(source);
                    targetProp.SetValue(destination, value);
                }
            }

            return destination;
        }

        public static T ConvertDictionaryToObject<T>(IDictionary<string, object> dictionary)
        {
            T obj = Activator.CreateInstance<T>();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (dictionary.ContainsKey(property.Name))
                {
                    var value = dictionary[property.Name];

                    if (value != null && property.CanWrite)
                    {
                        property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                    }
                }
            }

            return obj;
        }

        public static T TryJsonCasting<T>(string json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);

            json = json.Trim();

            bool isJson = json.StartsWith("{") && json.EndsWith("}") ||
                          json.StartsWith("[") && json.EndsWith("]") ||
                          json == "null";

            if (isJson)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch
                {
                    return default(T);
                }
            }

            try
            {
                return Convert.ChangeType(json, typeof(T)) as T;
            }
            catch
            {
                return null;
            }
        }
    }
}
