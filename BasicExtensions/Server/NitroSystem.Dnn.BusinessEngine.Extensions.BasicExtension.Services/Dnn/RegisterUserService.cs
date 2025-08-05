using Dapper;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.DnnServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Services.DnnServices
{
    public class RegisterUserService : ServiceBase<RegisterUserInfo>, IService
    {
        public override async Task<ServiceResult> ExecuteAsync<T>()
        {
            ServiceResult result = new ServiceResult();

            try
            {
                var firstName = this.ParseParam(this.Model.FirstName);
                var lastName = this.ParseParam(this.Model.LastName);
                var displayName = this.ParseParam(this.Model.DisplayName);
                var username = this.ParseParam(this.Model.Username);
                var email = this.ParseParam(this.Model.Email);
                var password = this.ParseParam(this.Model.Password);
                var repeatPassword = this.ParseParam(this.Model.RepeatPassword);

                var objUserInfo = new UserInfo()
                {
                    PortalID = this.PortalSettings.PortalId,
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
                objUserInfo.Membership.Approved = this.Model.IsApproved;

                var registeredStatus = UserController.CreateUser(ref objUserInfo);
                if (registeredStatus == UserCreateStatus.Success || registeredStatus == UserCreateStatus.AddUser || registeredStatus == UserCreateStatus.AddUserToPortal)
                {
                    result.IsError = false;
                    result.Data = objUserInfo.UserID;

                }
                else if (registeredStatus != UserCreateStatus.Success)
                {
                    result.IsError = true;
                    result.Data = registeredStatus;
                    result.ErrorException = new Exception(registeredStatus.ToString());
                }

                return result;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.ErrorException = ex;
            }

            return result;
        }

        public override bool TryParseModel(string serviceSettings)
        {
            throw new NotImplementedException();
        }
    }
}
