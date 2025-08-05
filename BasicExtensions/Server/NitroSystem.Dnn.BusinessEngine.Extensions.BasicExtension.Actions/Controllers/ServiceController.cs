using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Api.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Database;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.Database;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Repository;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Models;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Contract;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Mapping;
using NitroSystem.Dnn.BusinessEngine.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;
using NitroSystem.Dnn.BusinessEngine.Framework.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Dto;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.DnnServices;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.DnnServices;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.PublicServices;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.PublicServices;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.PublicServices.Webservice;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.PublicServices.SMSService;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Controllers
{
    public class ServiceController : DnnApiController
    {
        private readonly IModuleData _moduleData;
        private readonly IExpressionService _expressionService;
        private readonly IActionWorker _actionWorker;
        private readonly IServiceWorker _serviceWorker;

        public ServiceController(IModuleData moduleData, IActionWorker actionWorker, IServiceWorker serviceWorker, IExpressionService expressionService)
        {
            this._moduleData = moduleData;
            this._actionWorker = actionWorker;
            this._serviceWorker = serviceWorker;
            this._expressionService = expressionService;
        }

        #region Dabase Actions

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> CallSubmitEntityAction(ActionDto action)
        {
            try
            {
                this._moduleData.InitModuleData(action.ModuleID, action.ConnectionID, this.UserInfo.UserID, action.Form, action.Field, action.PageUrl);

                var actionController = new SubmitEntityAction(this._serviceWorker, this._actionWorker, action);

                var result = (ServiceResult)await actionController.ExecuteAsync<object>(false);
                if (result.IsError) throw result.ErrorException;

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> CallDataSourceAction(ActionDto action)
        {
            try
            {
                this._moduleData.InitModuleData(action.ModuleID, action.ConnectionID, this.UserInfo.UserID, action.Form, action.Field, action.PageUrl);

                var actionController = new DataSourceAction(this._serviceWorker, this._actionWorker, action);

                var result = (ServiceResult)await actionController.ExecuteAsync<object>(false);
                if (result.IsError) throw result.ErrorException;

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> CallBindEntityAction(ActionDto action)
        {
            try
            {
                this._moduleData.InitModuleData(action.ModuleID, action.ConnectionID, this.UserInfo.UserID, action.Form, action.Field, action.PageUrl);

                var actionController = new BindEntityAction(this._serviceWorker, this._actionWorker, action);

                var result = (ServiceResult)await actionController.ExecuteAsync<object>(false);
                if (result.IsError) throw result.ErrorException;

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> CallCustomQueryAction(ActionDto action)
        {
            try
            {
                this._moduleData.InitModuleData(action.ModuleID, action.ConnectionID, this.UserInfo.UserID, action.Form, action.Field, action.PageUrl);

                var actionController = new CustomQueryAction(this._serviceWorker, this._actionWorker, action);

                var result = (ServiceResult)await actionController.ExecuteAsync<object>(false);
                if (result.IsError) throw result.ErrorException;

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        #endregion

        #region Public Services Actions

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> SendSms(ActionDto action)
        {
            try
            {
                this._moduleData.InitModuleData(action.ModuleID, action.ConnectionID, this.UserInfo.UserID, action.Form, action.Field, action.PageUrl);

                var actionController = new SendSmsAction(this._serviceWorker, this._actionWorker, action);

                var result = (ServiceResult)await actionController.ExecuteAsync<SendSmsInfo>(false);
                if (result.IsError)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, result);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> SendEmail(ActionDto action)
        {
            try
            {
                this._moduleData.InitModuleData(action.ModuleID, action.ConnectionID, this.UserInfo.UserID, action.Form, action.Field, action.PageUrl);

                var actionController = new SendEmailAction(this._serviceWorker, this._actionWorker, action);

                var result = (ServiceResult)await actionController.ExecuteAsync<SendEmailInfo>(false);
                if (result.IsError)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, result);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        #endregion

        #region Dnn Service Actions

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> LoginUser(ActionDto action)
        {
            try
            {
                this._moduleData.InitModuleData(action.ModuleID, action.ConnectionID, this.UserInfo.UserID, action.Form, action.Field, action.PageUrl);

                var actionController = new LoginUserAction(this._serviceWorker, this._actionWorker, action);

                var result = (ServiceResult)await actionController.ExecuteAsync<LoginUserInfo>(false);
                if (result.IsError)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, result);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> RegisterUser(ActionDto action)
        {
            try
            {
                this._moduleData.InitModuleData(action.ModuleID, action.ConnectionID, this.UserInfo.UserID, action.Form, action.Field, action.PageUrl);

                var actionController = new LoginUserAction(this._serviceWorker, this._actionWorker, action);

                var result = (ServiceResult)await actionController.ExecuteAsync<RegisterUserInfo>(false);
                if (result.IsError)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, result);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> ResetPassword(ActionDto action)
        {
            try
            {
                this._moduleData.InitModuleData(action.ModuleID, action.ConnectionID, this.UserInfo.UserID, action.Form, action.Field, action.PageUrl);

                var actionController = new ResetPasswordAction(this._serviceWorker, this._actionWorker, action);

                var result = (ServiceResult)await actionController.ExecuteAsync<ResetPasswordInfo>(false);
                if (result.IsError)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, result);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        #endregion
    }
}
