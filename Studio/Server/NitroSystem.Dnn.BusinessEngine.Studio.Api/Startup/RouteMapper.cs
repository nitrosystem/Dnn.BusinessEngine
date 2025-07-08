using System.Web.Routing;
using DotNetNuke.Web.Api;
using System;
using System.Web.Http;
using System.ServiceModel.Activation;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Startup
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("BusinessEngineStudio", "default", "{controller}/{action}", new[] { "NitroSystem.Dnn.BusinessEngine.Studio.Api" });
           
            //GlobalConfiguration.Configuration.AddModuleInfoProvider(new StudioModuleInfoProvider());
        }
    }
}