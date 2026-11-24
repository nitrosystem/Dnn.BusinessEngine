using System.Net;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.WebServices
{
    public class WebServiceResult
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public object Headers { get; set; }
        public object Body { get; set; }
        public string BodyJson { get; set; }
    }
}
