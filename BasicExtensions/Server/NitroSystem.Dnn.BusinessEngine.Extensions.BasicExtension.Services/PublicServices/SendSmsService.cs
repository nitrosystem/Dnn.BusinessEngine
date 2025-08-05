using Dapper;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Providers.SmsGateway;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.DnnServices;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.PublicServices.SMSService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Services.PublicServices
{
    public class SendSmsService : ServiceBase<SendSmsInfo>, IService
    {
        public override async Task<ServiceResult> ExecuteAsync<T>()
        {
            ServiceResult result = new ServiceResult();

            try
            {
                var provider = this.ParseParam(this.Model.Provider);
                var mobile = this.ParseParam(this.Model.Mobile);
                var message = this.ParseParam(this.Model.Message);
                
                foreach (var token in this.Model.Tokens ?? Enumerable.Empty<TokenBase>())
                {
                    token.TokenValue = this.ParseParam((string)token.TokenValue);
                }

                SmsGatewayService.SendSms(this.PortalSettings, provider, mobile, message, this.Model.Tokens);

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
