using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.DependencyInjection;
using DotNetNuke.Data;
using System.Data.SqlClient;
using System.Data;
using System.Web.Routing;
using System.Net.WebSockets;
using System;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using System.Reflection;
using System.Linq;


namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.Startup
{
    internal class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var actionTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IAction).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in actionTypes)
                services.AddScoped(type);
        }
    }
}
