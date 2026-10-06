using DotNetNuke.Web.Api;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.Startup
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("BusinessEngineStudio", "default", "{controller}/{action}", new[] { "NitroSystem.Dnn.BusinessEngine.Studio.Api" });
        }
    }
}