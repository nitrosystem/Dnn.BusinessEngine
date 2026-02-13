using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Middlewares
{
    public class ResourcesMiddleware : IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>
    {
        public async Task<InstallExtensionResponse> InvokeAsync(IEngineContext context, InstallExtensionRequest request, Func<Task<InstallExtensionResponse>> next, Action<string, string, double> progress = null)
        {
            var unzipedPath = context.Get<string>("UnzipedPath");
            var unitOfWork = context.Get<IUnitOfWork>("UnitOfWork");

            foreach (var item in request.Manifest.Assemblies ?? Enumerable.Empty<ExtensionAssembly>())
            {
                string targetDir = Constants.MapPath(item.BasePath);
                foreach (var file in item.Items)
                {
                    string source = $@"{unzipedPath}\bin\{file}";
                    File.Copy(source, $@"{targetDir}\{file}", true);
                }
            }

            foreach (var item in request.Manifest.Resources ?? Enumerable.Empty<ExtensionResource>())
            {
                string zipFile = $@"{unzipedPath}\{item.ZipFile}";
                string targetDir = $@"{request.ModulePath}\{item.BasePath}";

                ZipProvider.Unzip(zipFile, targetDir);
            }

            unitOfWork.Commit();

            try
            {
                File.Delete(request.ExtensionZipFile);
                Directory.Delete(unzipedPath, true);
            }
            catch (Exception ex)
            {
            }

            var result = await next();
            return result;
        }
    }
}
