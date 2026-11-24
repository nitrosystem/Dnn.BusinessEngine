using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.Nitro_IExtensions.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IFieldTypePaneGeneration).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in types)
            {
                services.AddScoped(type);
            }
        }
    }
}
