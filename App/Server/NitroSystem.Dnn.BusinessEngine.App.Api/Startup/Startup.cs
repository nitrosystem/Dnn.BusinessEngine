using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.App.ApplicationService;
using NitroSystem.Dnn.BusinessEngine.App.Engine;


namespace NitroSystem.Dnn.BusinessEngine.App.Api.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBusinessEngineAppApplication();
            services.AddBusinessEngineAppEngine();
        }
    }
}
