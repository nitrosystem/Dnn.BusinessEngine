using System.Web.Routing;
using DotNetNuke.Web.Api;
using System;
using System.Web.Http;

namespace NitroSystem.Dnn.BusinessEngine.Api.Startup
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("BusinessEngine", "default", "{controller}/{action}", new[] { "NitroSystem.Dnn.BusinessEngine.Api" });

            //GlobalConfiguration.Configuration.AddModuleInfoProvider(new StudioModuleInfoProvider());
        }
    }
}