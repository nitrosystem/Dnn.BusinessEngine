using System.Linq;
using System.Net.Http;

namespace NitroSystem.Dnn.BusinessEngine.Core.WebApi
{
    public static class HttpRequestMessageExtensions
    {
        public static string GetHeaderValue(this HttpRequestMessage request, string headerName)
        {
            if (request.Headers.TryGetValues(headerName, out var values))
                return values.FirstOrDefault();

            return null;
        }
    }
}
