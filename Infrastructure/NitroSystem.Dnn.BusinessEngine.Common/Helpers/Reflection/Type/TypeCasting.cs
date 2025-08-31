using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Common.Reflection
{
    public static class TypeCasting
    {
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

        public static object CastVariableType(string typeString)
        {
            return Convert.ChangeType(typeString, Type.GetType("System." + typeString));
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
    }

}
