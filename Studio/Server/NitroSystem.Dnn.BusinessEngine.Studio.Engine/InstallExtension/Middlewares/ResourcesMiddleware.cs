using System;
using System.Linq;
using System.Threading;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension;
using System.IO;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Middlewares
{
    public class ResourcesMiddleware : IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>
    {
        public async Task<EngineResult<InstallExtensionResponse>> InvokeAsync(IEngineContext context, InstallExtensionRequest request, Func<Task<EngineResult<InstallExtensionResponse>>> next)
        {
            var ctx = context as InstallExtensionContext;

            foreach (var item in ctx.ExtensionManifest.Assemblies ?? Enumerable.Empty<ExtensionAssembly>())
            {
                string targetDir = Constants.MapPath(item.BasePath);
                foreach (var file in item.Items)
                {
                    string source = $@"{ctx.UnzipedPath}\bin\{file}";
                    File.Copy(source, $@"{targetDir}\{file}", true);
                }
            }

            foreach (var item in ctx.ExtensionManifest.Resources ?? Enumerable.Empty<ExtensionResource>())
            {
                string zipFile = $@"{ctx.UnzipedPath}\{item.ZipFile}";
                string targetDir = $@"{request.ModulePath}\{item.BasePath}";

                ZipProvider.Unzip(zipFile, targetDir);
            }

            ctx.UnitOfWork.Commit();

            try
            {
                File.Delete(request.ExtensionZipFile);
                Directory.Delete(ctx.UnzipedPath, true);
            }
            catch (Exception ex)
            {
            }

            var result = await next();
            return result;
        }
    }
}
