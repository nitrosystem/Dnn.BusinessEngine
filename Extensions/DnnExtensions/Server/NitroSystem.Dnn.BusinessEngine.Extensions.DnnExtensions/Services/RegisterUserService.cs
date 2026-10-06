using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Models;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.DnnExtensions.Services
{
    public class RegisterUserService
    {
        private readonly IServiceFactory _serviceFactory;

        public RegisterUserService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        // --- Constants to remove magic strings ---
        private static class UserKeys
        {
            public const string FirstName = "FirstName";
            public const string LastName = "LastName";
            public const string DisplayName = "DisplayName";
            public const string Email = "Email";
            public const string Username = "Username";
            public const string Password = "Password";
            public const string RepeatPassword = "RepeatPassword";
            public const string IsApproved = "IsApproved";
        }

        public async Task<UserRegistrationResult> RegisterUser(Guid serviceId, IReadOnlyList<ParamInfo> filledParams)
        {
            var service = await _serviceFactory.GetServiceViewModelAsync(serviceId)
                ?? throw new InvalidOperationException($"Service '{serviceId}' was not found.");

            var settings = service.Settings ?? new Dictionary<string, object>();
            var paramLookup = ParamHelper.GetLookup(filledParams);
            string firstName = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.FirstName), paramLookup);
            string lastName = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.LastName), paramLookup);
            string displayName = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.DisplayName), paramLookup);
            string email = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.Email), paramLookup);
            string username = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.Username), paramLookup);
            string password = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.Password), paramLookup);
            string repeatPassword = ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.RepeatPassword), paramLookup);
            bool.TryParse(ParamHelper.ResolveValue(settings.GetValueOrDefault(UserKeys.IsApproved), paramLookup), out var isApproved);

            var portalSettings = PortalSettings.Current;
            var objUserInfo = new UserInfo()
            {
                PortalID = portalSettings.PortalId,
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                DisplayName = !string.IsNullOrEmpty(displayName) ? displayName : (firstName + " " + lastName),
            };

            objUserInfo.Profile.FirstName = firstName;
            objUserInfo.Profile.LastName = lastName;

            objUserInfo.Membership.Password = password;
            objUserInfo.Membership.PasswordConfirm = repeatPassword;
            objUserInfo.Membership.Approved = isApproved;

            var registeredStatus = UserController.CreateUser(ref objUserInfo);
            return new UserRegistrationResult()
            {
                Status = registeredStatus,
                Id = objUserInfo.UserID
            };
        }
    }
}
