using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using DotNetNuke.Entities.Host;

namespace NitroSystem.Dnn.BusinessEngine.Utilities
{
    public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public CustomMultipartFormDataStreamProvider(string path) : base(path) { }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (headers.ContentDisposition.FileName == null) return base.GetStream(parent, headers);

            var filename = headers.ContentDisposition.FileName.Replace("\"", string.Empty);
            var fileExtension = Path.GetExtension(filename).ToLower();

            return Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(fileExtension) ? base.GetStream(parent, headers) : Stream.Null;
        }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            return headers.ContentDisposition.FileName.Replace("\"", string.Empty);
        }
    }
}