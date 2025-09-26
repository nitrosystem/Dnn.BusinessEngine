using Dapper;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
