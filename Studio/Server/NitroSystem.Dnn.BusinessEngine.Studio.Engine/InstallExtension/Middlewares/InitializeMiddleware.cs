using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Middlewares
{
    public class InitializeMiddleware : IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>
    {
        public async Task<InstallExtensionResponse> InvokeAsync(IEngineContext context, InstallExtensionRequest request, Func<Task<InstallExtensionResponse>> next, Action<string, string, double> progress = null)
        {
            var unzipedPath = Constants.MapPath($@"{request.BasePath}business-engine\temp\{Path.GetFileNameWithoutExtension(request.ExtensionZipFile)}");

            Directory.CreateDirectory(unzipedPath);

            var files = Directory.GetFiles(unzipedPath);
            ZipProvider.Unzip(request.ExtensionZipFile, unzipedPath);
            var manifestFile = files.FirstOrDefault(f => Path.GetFileName(f) == "manifest.json");
            var manifestJson = await FileUtil.GetFileContentAsync(manifestFile);

            context.Set<string>("UnzipedPath", unzipedPath);

            var result = await next();
            return result;
        }
    }
}