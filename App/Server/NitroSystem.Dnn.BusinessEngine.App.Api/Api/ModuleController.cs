using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using DotNetNuke.Web.Api;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.App.Api
{
    [AllowAnonymous]
    public class ModuleController : DnnApiController
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserDataStore _userDataStore;
        private readonly IDashboardService _dashboardService;
        private readonly IModuleVariableService _moduleVariableService;
        private readonly IModuleFieldService _moduleFieldService;
        private readonly IActionService _actionService;
        private readonly ActionRunner _actionRunner;

        public ModuleController(
            IServiceProvider serviceProvider,
            IUserDataStore userDataStore,
            IDashboardService dashboardService,
            IModuleVariableService moduleVariableService,
            IModuleFieldService moduleFieldService,
            IActionService actionService,
            ActionRunner actionRunner
        )
        {
            _serviceProvider = serviceProvider;
            _userDataStore = userDataStore;
            _moduleVariableService = moduleVariableService;
            _dashboardService = dashboardService;
            _moduleFieldService = moduleFieldService;
            _actionService = actionService;
            _actionRunner = actionRunner;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> GetModule(bool isDashboard, Guid moduleId, string connectionId, string pageUrl)
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
                        actions,
                        connectionId,
                        moduleId,
                        pageUrl,
                        PortalSettings.HomeSystemDirectoryMapPath,
                        UserInfo.UserID,
                        UserInfo.IsSuperUser,
                        UserInfo.Roles,
                        moduleData,
                        null);

                var data = _userDataStore.GetDataForClients(connectionId, moduleId) ?? new ConcurrentDictionary<string, object>();
                var variables = await _moduleVariableService.GetVariablesAsync(moduleId, ModuleVariableScope.Global, ModuleVariableScope.ClientSide);
                var fields = await _moduleFieldService.GetFieldsDtoAsync(moduleId);
                var clientActions = await _actionService.GetActionsDtoForClientAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    data,
                    dashboard,
                    fields,
                    variables,
                    actions = clientActions.Select(a => new { a.Id, a.FieldId, a.ActionName, a.Event }),
                    pageActions = actions
                });

                //return Request.CreateResponse(HttpStatusCode.OK, new
                //{
                //    f = GlobalHelper.ProtectPayload(fields),
                //    a = GlobalHelper.ProtectPayload(actions),
                //    v = GlobalHelper.ProtectPayload(variables),
                //    d = GlobalHelper.ProtectPayload(data)
                //});
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> ExecuteAction(ActionDto action)
        {
            try
            {
                var moduleData = await _userDataStore.UpdateModuleDataAsync(action.ConnectionId, action.ModuleId, action.Data, PortalSettings.HomeSystemDirectoryMapPath);
                var actions = await _actionService.GetActionsAsync(action.ModuleId, action.FieldId, action.ActionId, action.Event, ModuleEventTriggerOn.PageLoadOrComponentBehavior);
                var items = await _actionRunner.ExecuteAsync(
                        actions,
                        action.ConnectionId,
                        action.ModuleId,
                        action.PageUrl,
                        PortalSettings.HomeSystemDirectoryMapPath,
                        UserInfo.UserID,
                        UserInfo.IsSuperUser,
                        UserInfo.Roles,
                        moduleData,
                        action.ExtraParams
                    );

                var data = _userDataStore.GetDataForClients(action.ConnectionId, action.ModuleId) ?? new ConcurrentDictionary<string, object>();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    items.IsRequiredToUpdateData,
                    items.Results,
                    Data = data,
                    FirstActionId = items.Results.FirstOrDefault()?.Id
                });
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
    }
}