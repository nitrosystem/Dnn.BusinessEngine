using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core;
using NitroSystem.Dnn.BusinessEngine.Repository;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBusinessEngineCore();
            services.AddBusinessEngineRepository();
            services.AddBusinessEngineStudioEngine();
            services.AddBusinessEngineStudioApplication();
        }
    }
}
