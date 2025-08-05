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
    public class ResetPasswordService : ServiceBase<ResetPasswordInfo>, IService
    {
        public override async Task<ServiceResult> ExecuteAsync<T>()
        {
            ServiceResult result = new ServiceResult();

            try
            {
                var username = this.ParseParam(this.Model.Username);
                var password = this.ParseParam(this.Model.Password);

                var user = UserController.GetUserByName(this.PortalSettings.PortalId, username);

                var pass = MembershipProvider.Instance().ResetPassword(user, string.Empty);

                result.Data = 1;

                if (!MembershipProvider.Instance().ChangePassword(user, pass, password))
                {
                    result.IsError = true;
                    result.Data = 0;
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
