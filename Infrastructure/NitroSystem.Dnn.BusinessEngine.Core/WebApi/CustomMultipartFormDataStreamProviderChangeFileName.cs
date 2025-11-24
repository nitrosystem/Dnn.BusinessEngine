using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using DotNetNuke.Entities.Host;

namespace NitroSystem.Dnn.BusinessEngine.Core.WebApi
{
    public class CustomMultipartFormDataStreamProviderChangeFileName : MultipartFormDataStreamProvider
    {
        public CustomMultipartFormDataStreamProviderChangeFileName(string path)
            : base(path) { }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (headers.ContentDisposition.FileName == null) return base.GetStream(parent, headers);

            var filename = headers.ContentDisposition.FileName.Replace("\"", string.Empty);
            var fileExtension = Path.GetExtension(filename).ToLower();

            // Extension whitelist check
            if (!Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(fileExtension))
                throw new Exception($"File type '{fileExtension}' is not allowed.");

            return base.GetStream(parent, headers);
        }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            // override the filename which is stored by the provider (by default is bodypart_x)
            string oldfileName = headers.ContentDisposition.FileName.Replace("\"", string.Empty);
            string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(oldfileName);

            return newFileName;
        }
    }
}