using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection
{
    public static class DictionaryToObjectConverter
    {
        public static T ConvertToObject<T>(IDictionary<string, object> dictionary)
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
