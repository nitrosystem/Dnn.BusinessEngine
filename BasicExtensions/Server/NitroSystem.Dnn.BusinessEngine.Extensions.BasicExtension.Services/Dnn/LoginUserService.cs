using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DnnServices
{
    public class LoginUserService : ILoginUserService
    {
        public UserLoginStatus LoginUser(string username, string password, PortalSettings portalSettings)
        {
            string ip = HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : "";

            UserLoginStatus status = UserLoginStatus.LOGIN_FAILURE;

            var user = UserController.ValidateUser(portalSettings.PortalId, username, password, string.Empty, string.Empty, ip, ref status);
            if (status == UserLoginStatus.LOGIN_SUCCESS || status == UserLoginStatus.LOGIN_SUPERUSER)
            {
                UserController.UserLogin(portalSettings.PortalId, user, string.Empty, ip, true);
            }

            return status;
        }
    }
}
