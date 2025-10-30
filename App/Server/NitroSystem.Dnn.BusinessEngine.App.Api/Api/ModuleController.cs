using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.App.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionEngine;
using System.Runtime.InteropServices;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts;
using System.Linq;
using System.Collections.Concurrent;

namespace NitroSystem.Dnn.BusinessEngine.App.Api
{
    public class ModuleController : DnnApiController
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserDataStore _userDataStore;
        private readonly IBuildBufferService _buildBufferService;
        private readonly IDashboardService _dashboardService;
        private readonly IModuleService _moduleService;
        private readonly IActionService _actionService;

        public ModuleController(
            IServiceProvider serviceProvider,
            IBuildBufferService buildBufferService,
            IUserDataStore userDataStore,
            IDashboardService dashboardService,
            IModuleService moduleService,
            IActionService actionService
        )
        {
            _serviceProvider = serviceProvider;
            _userDataStore = userDataStore;
            _buildBufferService = buildBufferService;
            _moduleService = moduleService;
            _dashboardService = dashboardService;
            _actionService = actionService;
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

                var moduleData = new ConcurrentDictionary<string, object>();
                var actionIds = await _actionService.GetActionIdsAsync(moduleId, null, "OnPageLoad");
                if (actionIds.Any())
                {
                    var actionsByEvent = await _actionService.GetActionsDtoForServerAsync(actionIds);
                    var request = new ActionRequest()
                    {
                        ConnectionId = connectionId,
                        ModuleId = moduleId,
                        PageUrl = pageUrl,
                        ByEvent = true,
                        Actions = actionsByEvent
                    };
                    var actionEngine = new ActionExecutionEngine(_serviceProvider, _userDataStore, _buildBufferService);
                    var response = await actionEngine.ExecuteAsync(request);

                    moduleData = response.Data.ModuleData;
                }

                var data = _userDataStore.GetDataForClients(moduleId, moduleData);

                var variables = await _moduleService.GetModuleVariables(moduleId, ModuleVariableScope.Global);
                var fields = await _moduleService.GetFieldsViewModelAsync(moduleId);
                var actions = await _actionService.GetActionsDtoForClientAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    dashboard,
                    fields,
                    actions,
                    data,
                    variables
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
                var moduleData = await _userDataStore.UpdateModuleData(action.ConnectionId, action.ModuleId, action.Data);
                moduleData["_PageParam"] = UrlHelper.ParsePageParameters(action.PageUrl);

                //await _actionWorker.CallActions(action.ActionIds, moduleData);

                var data = _userDataStore.GetDataForClients(action.ModuleId, moduleData);

                return Request.CreateResponse(HttpStatusCode.OK, new { data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage PingConnection(ActionDto user)
        {
            try
            {
                _userDataStore.Ping(user.ConnectionId);

                return Request.CreateResponse(HttpStatusCode.OK, true);
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