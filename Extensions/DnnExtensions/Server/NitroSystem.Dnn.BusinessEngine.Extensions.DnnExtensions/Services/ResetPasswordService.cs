using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Services
{
    public class ResetPasswordService
    {
        private readonly IServiceFactory _serviceFactory;

        public ResetPasswordService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        // --- Constants to remove magic strings ---
        private static class UserKeys
        {
            public const string Username = "Username";
            public const string Password = "Password";
			public const string RepeatPassword = "RepeatPassword";
        }

        public async Task<bool> ResetPassword(Guid serviceId, IReadOnlyList<ParamInfo> filledParams)
        {
            var service = await _serviceFactory.GetServiceViewModelAsync(serviceId)
                ?? throw new InvalidOperationException($"Service '{serviceId}' was not found.");

            var settings = service.Settings ?? new Dictionary<string, object>();
            var paramLookup = ParamHelper.GetLookup(filledParams);
            string username = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.Username), paramLookup);
            string password = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.Password), paramLookup);
            string repeatPassword = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.RepeatPassword), paramLookup);

            var portalSettings = PortalSettings.Current;
            var user = UserController.GetUserByName(portalSettings.PortalId, username);
            var newPassword = MembershipProvider.Instance().ResetPassword(user, string.Empty);
            if (!MembershipProvider.Instance().ChangePassword(user, newPassword, password))
                return false;

            return true;
        }
    }
}
