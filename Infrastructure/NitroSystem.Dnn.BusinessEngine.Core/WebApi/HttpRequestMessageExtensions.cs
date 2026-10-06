using System;
using System.Linq;
using System.Net.Http;

namespace NitroSystem.Dnn.BusinessEngine.Core.WebApi
{
    public static class HttpRequestMessageExtensions
    {
        public static T GetHeaderValue<T>(this HttpRequestMessage request, string headerName)
        {
            if (request.Headers.TryGetValues(headerName, out var values))
            {
                object value = values.FirstOrDefault();
                if (string.IsNullOrEmpty(value?.ToString()))
                    return default(T);
                else
                {
                    if (typeof(T) == typeof(Guid))
                        return (T)(object)Guid.Parse(value.ToString());

                    return (T)value;
                }
            }

            return default(T);
        }
    }
}
