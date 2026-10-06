using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Services;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var actionTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IActionExecutor).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in actionTypes)
                services.AddScoped(type);

            services.AddTransient<LoginUserService>();
            services.AddTransient<RegisterUserService>();
            services.AddTransient<ResetPasswordService>();
            services.AddTransient<UpdateUserService>();
        }
    }
}
