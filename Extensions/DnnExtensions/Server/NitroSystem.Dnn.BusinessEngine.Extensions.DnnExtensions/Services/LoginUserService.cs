using System;
using System.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Models;
using System.Web.Security;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Services
{
    public class LoginUserService
    {
        private readonly IServiceFactory _serviceFactory;

        public LoginUserService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        // --- Constants to remove magic strings ---
        private static class UserKeys
        {
            public const string Username = "Username";
            public const string Password = "Password";
            public const string RememberMe = "RememberMe";
        }

        public async Task<UserLoginResult> LoginUser(Guid serviceId, IReadOnlyList<ParamInfo> filledParams)
        {
            var service = await _serviceFactory.GetServiceViewModelAsync(serviceId)
                ?? throw new InvalidOperationException($"Service '{serviceId}' was not found.");


            var settings = service.Settings ?? new Dictionary<string, object>();
            var paramLookup = ParamHelper.GetLookup(filledParams);
            string username = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.Username), paramLookup);
            string password = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.Password), paramLookup);
            bool.TryParse(ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.RememberMe), paramLookup), out var rememberMe);

            UserLoginStatus status = UserLoginStatus.LOGIN_FAILURE;
            var portalSettings = PortalSettings.Current;
            var ip = HttpContext.Current != null
                ? HttpContext.Current.Request.UserHostAddress
                : "";

            if (GlobalHelper.IsValidEmail(username))
            {
                var _username = UserController.GetUserByEmail(portalSettings.PortalId, username)?.Username;
                if (!string.IsNullOrEmpty(_username)) username = _username;
            }

            var user = UserController.ValidateUser(portalSettings.PortalId, username, password, string.Empty, string.Empty, ip, ref status);
            if (status == UserLoginStatus.LOGIN_SUCCESS || status == UserLoginStatus.LOGIN_SUPERUSER)
            {
                UserController.UserLogin(portalSettings.PortalId, user, string.Empty, ip, rememberMe);

                if (rememberMe)
                {
                    FormsAuthenticationTicket ticket =
                        new FormsAuthenticationTicket(1, username, DateTime.Now, DateTime.Now.AddDays(30), true, "", FormsAuthentication.FormsCookiePath);

                    // Ticket encryption
                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                    // Create cookie
                    HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    cookie.Expires = ticket.Expiration;
                    cookie.HttpOnly = true;
                    cookie.Secure = true;
                    cookie.Path = FormsAuthentication.FormsCookiePath;
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }

            return new UserLoginResult()
            {
                Status = status,
                Message = ""
            };
        }
    }
}
