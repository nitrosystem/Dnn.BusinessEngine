using System;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Middlewares
{
    public class ValidateMiddleware : IEngineMiddleware<InstallExtensionRequest, InstallExtensionResponse>
    {
        private readonly IExtensionService _extensionService;

        public ValidateMiddleware(IExtensionService extensionService)
        {
            _extensionService = extensionService;
        }

        public async Task<InstallExtensionResponse> InvokeAsync(IEngineContext context, InstallExtensionRequest request, Func<Task<InstallExtensionResponse>> next, Action<string, string, double> progress = null)
        {
            var isValid = true;
            var errors = new StringBuilder();

            var user = UserController.Instance.GetCurrentUserInfo();
            if (!user.IsSuperUser)
            {
                isValid = false;
                errors.AppendLine("Only superusers can install extensions.");
            }

            var currentVersion = await _extensionService.GetCurrentVersionExtensionsAsync(request.Manifest.ExtensionName);
            if (!string.IsNullOrEmpty(currentVersion) && new Version(currentVersion) > new Version(request.Manifest.Version))
            {
                isValid = false;
                errors.AppendLine("The installed extension should not be larger than the new extension");
            }

            if(!isValid)
            {
                context.CancellationTokenSource.Cancel();
                return default;
            }

            context.Set<string>("CurrentVersion", currentVersion);
            context.Set<string>("IsNewExtension" , currentVersion);

            var result = await next();
            return result;
        }
    }
}
