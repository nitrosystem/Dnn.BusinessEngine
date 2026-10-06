using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.Services;


namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var actionTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IActionExecutor).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in actionTypes)
                services.AddScoped(type);

            services.AddTransient<Services.BindEntityService>();
            services.AddTransient<Services.DataRowService>();
            services.AddTransient<Services.DataSourceService>();
            services.AddTransient<Services.SubmitEntityService>();
            services.AddTransient<Services.DeleteEntityRowService>();
            services.AddTransient<Services.CustomQueryService>();

            services.AddScoped<BindEntityService>();
            services.AddScoped<DataRowService>();
            services.AddScoped<DataSourceService>();
            services.AddScoped<SubmitEntityService>();
            services.AddScoped<DeleteEntityRowService>();
            services.AddScoped<CustomQueryService>();

            ServiceMappingProfile.Register();
        }
    }
}
