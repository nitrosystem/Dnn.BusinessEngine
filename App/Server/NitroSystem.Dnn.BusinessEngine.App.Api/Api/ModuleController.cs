using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotNetNuke.Web.Api;
using NitroSystem.Dnn.BusinessEngine.App.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using System.Collections.Concurrent;
using System.Linq;
using NitroSystem.Dnn.BusinessEngine.Core.WebApi;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;

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
                    ? await _dashboardService.GetDashboardDtoAsync(moduleId, UserInfo.IsSuperUser, UserInfo.Roles)
                    : null;

                var moduleData = await _userDataStore.GetOrCreateModuleDataAsync(connectionId, moduleId, PortalSettings.HomeSystemDirectoryMapPath);
                var actions = await _actionService.GetActionsAsync(moduleId);
                if (actions.Count > 0)
                    await _actionRunner.ExecuteAsync(
                        actions.Where(a => a.AuthorizationRunAction == null || a.AuthorizationRunAction.Any(r => UserInfo.IsInRole(r))).ToList(),
                        connectionId,
                        moduleId,
                        UserInfo.UserID,
                        pageUrl,
                        PortalSettings.HomeSystemDirectoryMapPath,
                        moduleData,
                        null);

                var data = _userDataStore.GetDataForClients(connectionId, moduleId) ?? new ConcurrentDictionary<string, object>();
                var variables = await _moduleService.GetVariables(moduleId, ModuleVariableScope.Global, ModuleVariableScope.ClientSide);
                var fields = await _moduleService.GetFieldsDtoAsync(moduleId);
                var clientActions = await _actionService.GetActionsDtoForClientAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    data,
                    dashboard,
                    fields,
                    variables,
                    actions = clientActions.Select(a => new { a.FieldId, a.Event }),
                    pageActions = actions
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
        public async Task<HttpResponseMessage> CallFieldAction(ActionDto action)
        {
            try
            {
                var moduleData = await _userDataStore.UpdateModuleData(action.ConnectionId, action.ModuleId, action.Data, PortalSettings.HomeSystemDirectoryMapPath);
                var actions = await _actionService.GetActionsAsync(action.ModuleId, action.FieldId, action.ActionId, action.Event, ModuleEventTriggerOn.PageLoadOrComponentBehavior);
                var items = await _actionRunner.ExecuteAsync(
                        actions.Where(a => a.AuthorizationRunAction == null || a.AuthorizationRunAction.Any(r => UserInfo.IsInRole(r))).ToList(),
                        action.ConnectionId,
                        action.ModuleId,
                        UserInfo.UserID,
                        action.PageUrl,
                        PortalSettings.HomeSystemDirectoryMapPath,
                        moduleData,
                        action.ExtraParams
                    );

                var data = _userDataStore.GetDataForClients(action.ConnectionId, action.ModuleId) ?? new ConcurrentDictionary<string, object>();

                return Request.CreateResponse(HttpStatusCode.OK, new { items.Results, items.IsRequiredToUpdateData, Data = data });
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