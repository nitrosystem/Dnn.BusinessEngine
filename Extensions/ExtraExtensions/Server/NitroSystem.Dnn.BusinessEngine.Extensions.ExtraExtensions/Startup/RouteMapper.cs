using DotNetNuke.Web.Api;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Startup
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("BE_ExtraExtensions", "default", "{controller}/{action}", 
                new[] { "NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Api" });
        }
    }
}