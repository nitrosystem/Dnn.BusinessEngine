using DotNetNuke.Web.Api;

namespace NitroSystem.Dnn.BusinessEngine.App.Api.Startup
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("BusinessEngineApp", "default", "{controller}/{action}", new[] { "NitroSystem.Dnn.BusinessEngine.App.Api" });
        }
    }
}