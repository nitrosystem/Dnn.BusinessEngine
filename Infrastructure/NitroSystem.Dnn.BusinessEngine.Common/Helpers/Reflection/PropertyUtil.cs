using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Reflection
{
    public static class PropertyUtil
    {
        public static bool HasProperty(this object source, string name)
        {
            return source.GetType().GetProperty(name) != null;
        }

        public static PropertyInfo GetPropertyByName(this object source, string name)
        {
            return source.GetType().GetProperty(name);
        }

        public static object GetPropertyValueByName(this object source, string name)
        {
            var prop = GetPropertyByName(source, name);
            return prop.GetValue(source, null);
        }

        public static void SetPropertyValueByName(this object source, string name, object value)
        {
            var prop = GetPropertyByName(source, name);
            prop.SetValue(source, value);
        }
    }
}
