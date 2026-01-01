using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using DotNetNuke.Web.Api;
using NitroSystem.Dnn.BusinessEngine.App.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionOrchestrator;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution;

namespace NitroSystem.Dnn.BusinessEngine.App.Api
{
    public class ModuleController : DnnApiController
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserDataStore _userDataStore;
        private readonly IDashboardService _dashboardService;
        private readonly IModuleService _moduleService;
        private readonly IActionService _actionService;
        private readonly ActionRunner _actionRunner;

        public ModuleController(
            IServiceProvider serviceProvider,
            IUserDataStore userDataStore,
            IDashboardService dashboardService,
            IModuleService moduleService,
            IActionService actionService,
            ActionRunner actionRunner
        )
        {
            _serviceProvider = serviceProvider;
            _userDataStore = userDataStore;
            _moduleService = moduleService;
            _dashboardService = dashboardService;
            _actionService = actionService;
            _actionRunner = actionRunner;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> GetModule(Guid moduleId, bool isDashboard, string connectionId, string pageUrl)
        {
            try
            {
                var dashboard = isDashboard
                    ? await _dashboardService.GetDashboardDtoAsync(moduleId)
                    : null;

                var moduleData = await _userDataStore.GetOrCreateModuleDataAsync(connectionId, moduleId, PortalSettings.HomeSystemDirectoryMapPath);
                moduleData["_PageParam"] = UrlHelper.ParsePageParameters(pageUrl);
                moduleData["_CurrentUserId"] = UserInfo.UserID;

                List<ActionResponse> actionResponses = null;
                var actionIds = await _actionService.GetActionIdsAsync(moduleId, null, "OnPageLoad");
                if (actionIds.Any())
                {
                    actionResponses = await _actionRunner.RunAsync(
                        actionIds,
                        connectionId,
                        moduleId,
                        PortalSettings.HomeSystemDirectoryMapPath,
                        moduleData
                    );
                }

                var data = _userDataStore.GetDataForClients(connectionId, moduleId);
                var variables = await _moduleService.GetVariables(moduleId, ModuleVariableScope.Global, ModuleVariableScope.ClientSide);
                var fields = await _moduleService.GetFieldsDtoAsync(moduleId);
                var actions = await _actionService.GetActionsDtoForClientAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    dashboard,
                    fields,
                    actions,
                    data,
                    variables,
                    actionResponses
                });

                //return Request.CreateResponse(HttpStatusCode.OK, new
                //{
                //    mf = ProtectPayload(fields),
                //    ma = ProtectPayload(actions),
                //    md = ProtectPayload(data)
                //});
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> CallAction(ActionDto action)
        {
            try
            {
                var moduleData = await _userDataStore.UpdateModuleData(action.ConnectionId, action.ModuleId, action.Data, PortalSettings.HomeSystemDirectoryMapPath);
                moduleData["_PageParam"] = UrlHelper.ParsePageParameters(action.PageUrl);
                moduleData["_CurrentUserId"] = UserInfo.UserID;

                var actionResponses = await _actionRunner.RunAsync(
                        action.ActionIds,
                        action.ConnectionId,
                        action.ModuleId,
                        PortalSettings.HomeSystemDirectoryMapPath,
                        moduleData,
                        action.ExtraParams
                    );

                return Request.CreateResponse(HttpStatusCode.OK, actionResponses);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage DisconnectUser([FromBody] ConnectionUserDto user)
        {
            try
            {
                _userDataStore.DisconnectUser(user.ConnectionId, user.ModuleId);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static string ProtectPayload(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);

            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionMode.Compress))
                using (var writer = new StreamWriter(gzip))
                {
                    writer.Write(json);
                }

                return Convert.ToBase64String(output.ToArray());
            }
        }
    }
}