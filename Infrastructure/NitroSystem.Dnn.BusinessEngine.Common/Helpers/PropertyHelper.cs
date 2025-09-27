using System.Reflection;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Helpers
{
    public static class PropertyHelper
    {
        public static bool HasProperty(this object source, string name)
        {
            return source?.GetType().GetProperty(name) != null;
        }

        public static PropertyInfo GetPropertyByName(object source, string name)
        {
            return source?.GetType().GetProperty(name);
        }

        public static object GetPropertyValueByName(object source, string name)
        {
            var prop = GetPropertyByName(source, name);
            return prop?.GetValue(source);
        }

        public static void SetPropertyValueByName(object source, string name, object value)
        {
            var prop = GetPropertyByName(source, name);
            if (prop?.CanWrite == true)
            {
                prop.SetValue(source, value);
            }
        }
    }
}
