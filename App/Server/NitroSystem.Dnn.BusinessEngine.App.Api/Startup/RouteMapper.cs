using DotNetNuke.Web.Api;

namespace NitroSystem.Dnn.BusinessEngine.Api.Startup
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("BusinessEngine", "default", "{controller}/{action}", new[] { "NitroSystem.Dnn.BusinessEngine.App.Api" });
        }
    }
}