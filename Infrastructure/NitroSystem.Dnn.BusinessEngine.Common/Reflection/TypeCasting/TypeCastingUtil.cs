using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Common.TypeCasting
{
    public static class TypeCastingUtil<T> where T : class
    {
        public static T TryJsonCasting(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

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
                    return null;
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
