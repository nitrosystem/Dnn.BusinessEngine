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
        public async Task<InstallExtensionResponse> InvokeAsync(IEngineContext context, InstallExtensionRequest request, Func<Task<InstallExtensionResponse>> next, Action<string, double> progress = null)
        {
            var unitOfWork = context.Get<IUnitOfWork>("UnitOfWork");

            progress("Start copy assemblies", 70);

            foreach (var item in request.Manifest.Assemblies ?? Enumerable.Empty<ExtensionAssembly>())
            {
                string targetDir = Constants.MapPath(item.BasePath);
                foreach (var file in item.Items)
                {
                    string source = $@"{request.ExtractPath}\bin\{file}";
                    File.Copy(source, $@"{targetDir}\{file}", true);
                }
            }

            progress("end copy assemblies", 75);

            progress("Start copy resources", 80);

            foreach (var item in request.Manifest.Resources ?? Enumerable.Empty<ExtensionResource>())
            {
                string zipFile = $@"{request.ExtractPath}\{item.ZipFile}";
                string targetDir = $@"{request.ModulePath}\{item.BasePath}";

                ZipProvider.Unzip(zipFile, targetDir);
            }

            progress("Start copy resources", 95);

            unitOfWork.Commit();

            try
            {
                //File.Delete(request.ExtensionZipFile);
                //Directory.Delete(request.ExtractPath, true);
            }
            catch (Exception ex)
            {
            }

            return new InstallExtensionResponse() { IsSuccess = true };
        }
    }
}
